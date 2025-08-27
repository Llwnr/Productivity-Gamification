importScripts(['chromeHelper.js'])

var activeTabId = null;
var activeFullUrl = null;

chrome.runtime.onInstalled.addListener(async () => {
	await notifyLastActiveTime();
	setInterval(setLatestActiveTime, 1000); //Store browser active time & update it regularly
})

chrome.runtime.onStartup.addListener(async () => {
	await notifyLastActiveTime();
	setInterval(setLatestActiveTime, 1000);
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
	// clearTimeout(siteAnalysisDebouncer);
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

//Monitor for browser inactivity i.e. browser being not the main focused window.
setInterval(() => {
	chrome.windows.getLastFocused((window) => {
		if(window && window.focused){
			//Do nothing as browser is being focused
		}else{
			clearTimeout(siteAnalysisDebouncer);
			notifyBrowsingStopped();
		}
	})
},2000)

// chrome.runtime.onMessage.addListener(async (message, sender, sendResponse) => {
//   if (message.type === 'tab_focused') {
//   	sendMessage("Wat da heck");
//   	const [activeTab] = await chrome.tabs.query({active: true, currentWindow: true});
// 		sendMessage('Okay tab is active: ' + sender.tab.url);
//   } else if (message.type === 'tab_blurred') {
//     // sendMessage('User switched away from tab:' + sender.tab.url);
//     // Add your logic here for when the browser loses focus
//   }
// });