importScripts(['chromeHelper.js'])

const API_BASE_URL = "https://localhost:7131/SiteMonitor";
const API_ENDPOINT = "AnalyzeSite";

const userGoal = "Game Developer";

var activeTabId = null;
var activeFullUrl = null;

chrome.runtime.onInstalled.addListener(async () => {
	await manageUnwantedShutdown();
})

chrome.runtime.onStartup.addListener(async () => {
	await manageUnwantedShutdown();
})

chrome.tabs.onCreated.addListener((tab) => {
    notifyBrowsingStopped();
});

//Listen for history state updates i.e when SPA sites like youtube go from youtube.com to youtube.com/searchedItem without a full reload
chrome.webNavigation.onHistoryStateUpdated.addListener(async (details) => {
	if (details.frameId == 0 && await isTabCurrentlyActive(details.tabId, details.url)){
	    setSiteVisited(details.url, details.tabId, "historyStateUpdated");
    }
})

//For sites that redirect, only send url data once the redirection finishes i.e. final site data
chrome.webNavigation.onCompleted.addListener(async (details) => {
	//When the site finishes loading, check if its the active tab.
	if (details.frameId == 0 && await isTabCurrentlyActive(details.tabId, details.url)){ 
		setSiteVisited(details.url, details.tabId, "siteLoaded");
	}
});

//When user switches tabs, notify server that user is visiting a different site now
chrome.tabs.onActivated.addListener(async (details) => {
	// details contains tabId and windowId
	const tab = await chrome.tabs.get(details.tabId);
	if (tab && tab.url) {
		if (tab.status === "complete" && await isTabCurrentlyActive(tab.id, tab.url)) { // Avoid sending new tab page immediately
			setSiteVisited(tab.url, tab.id, "tabSwitched");
		}
	}
})

//When user switches windows or tabs
chrome.windows.onFocusChanged.addListener(async (windowId) => {
    if(windowId === chrome.windows.WINDOW_ID_NONE) return;
	try{
		// Get the currently active tab in the newly focused window
		const [activeTab] = await chrome.tabs.query({ active: true, windowId: windowId });
		if (activeTab && activeTab.url && activeTab.status === "complete") {
			setSiteVisited(activeTab.url, activeTab.id, "windowSwitched");
		}
	}catch(ex){
		console.log("Error: " + ex);
	}
});

chrome.tabs.onRemoved.addListener((tabId, removeInfo) => {
	if(removeInfo.isWindowClosing){
		chrome.storage.local.set({browserClosedNormally: true})
	}
})

chrome.windows.onRemoved.addListener(() => {
	chrome.storage.local.set({browserClosedNormally: true})
})