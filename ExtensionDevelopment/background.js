const API_BASE_URL = "http://localhost:5160/SiteMonitor";
const API_ENDPOINT = "AnalyzeSite";

const userGoal = "Game Developer";

var activeTabId = null;
var activeFullUrl = null;

async function getCurrentlyActiveTabInfo(){
    try{
        const [tab] = await chrome.tabs.query({active: true, lastFocusedWindow: true});
        return tab;
    }catch(error){
        console.log("Error getting active tab info:", error);
        return null;
    }
}

async function isTabCurrentlyActive(tabId, url){
    const activeTab = await getCurrentlyActiveTabInfo();
    // Ensure the tab exists, its ID matches, and its URL matches (important for redirects/history updates)
    return activeTab && activeTab.id === tabId && activeTab.url === url;
}

function getTitleAndDescription(tabId, callback) {
  chrome.scripting.executeScript({
    target: { tabId },
    func: () => {
      const descTag = document.querySelector('meta[name="description"]');
      const titleTag = document.querySelector('title');
      const tags = {
        "description" : descTag ? descTag.content : null,
        "title" : titleTag ? titleTag.innerText : null
      }
      return tags
    }
  }, ([result]) => callback(result?.result));
}

async function getIdToken(){
    return await chrome.storage.local.get(['authToken']).authToken;
}

var siteAnalysisDebouncer = 99999999; 
async function setSiteVisited(url, tabId, triggerType) {
    let token = await getIdToken();
    if(token == null){
        console.log("Token is null");
        return;
    }
    if(!url) return;
    //Check if tab has been switched to a non browsable url i.e. browser setting pages etc
    if(triggerType == 'tabSwitched' && !(url.startsWith('http://') || url.startsWith('https://'))){
        await notifyBrowsingStopped();
        return;
    }

	//Check if its a valid browsable url and not the browser's settings stuff
	if (!url.startsWith('http://') && !url.startsWith('https://')) return;
	if (activeFullUrl == url && activeTabId == tabId) return; //Don't do anything if same page and same tab

	clearTimeout(siteAnalysisDebouncer);
	// let urlWithDetail = tabId + " " + triggerType + " " + url;
    siteAnalysisDebouncer = setTimeout(()=>{
		getTitleAndDescription(tabId, (tags) => {
			const title = (tags.title == null || tags.title.length > 0) ? tags.title : "null";
			const desc = (tags.description == null || tags.description.length) > 0 ? tags.description : "null";
            const api_url = `${API_BASE_URL}/${API_ENDPOINT}?userGoal=${encodeURIComponent(userGoal)}&url=${encodeURIComponent(url)}&title=${encodeURIComponent(title)}&desc=${encodeURIComponent(desc)}`;
			fetch(api_url, {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${token}`
                }
            })
			.then(response => {
				if (!response.ok) {
					console.error(`API call failed with status: ${response.status}\nUrl:${api_url}`);
				}
			})
			.catch(error => {
				console.error(`Error fetching SiteBrowsed API: ${error}`);
			});
		})
		},1000);

	activeTabId = tabId;
	activeFullUrl = url
}

async function notifyBrowsingStopped(){
    var notifyBrowsingStopped = `${API_BASE_URL}/BrowsingStopped`;
    let token = await getIdToken();
    fetch(notifyBrowsingStopped, {
        method: 'GET',
        headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${token}`
        }
    })

    activeTabId = null;
    activeFullUrl = null;
}

chrome.tabs.onCreated.addListener((tab) => {
    notifyBrowsingStopped();
});

//Listen for history state updates i.e when SPA sites like youtube go from youtube.com to youtube.com/searchedItem without a full reload
chrome.webNavigation.onHistoryStateUpdated.addListener(async (details) => {
	if (details.frameId == 0 && await isTabCurrentlyActive(details.tabId, details.url)){
	    setSiteVisited(details.url, details.tabId, "historyStateUpdated");
    }
})

//For sites that redirect
chrome.webNavigation.onCompleted.addListener(async (details) => {
	//When the site finishes loading, check if its the active tab.
	if (details.frameId == 0 && await isTabCurrentlyActive(details.tabId, details.url)){ 
		setSiteVisited(details.url, details.tabId, "siteLoaded");
	}
});

//When user switches tabs
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

//When browsed is closed
//NATIVE HOST SETUP
const NATIVE_HOST_NAME = "com.productivity_gamification.console_app"
let port = null;

function connectToNativeHost() {
    console.log(`Attempting to connect to native host: ${NATIVE_HOST_NAME}`);
    port = chrome.runtime.connectNative(NATIVE_HOST_NAME);

    port.onMessage.addListener((message) => {
        console.log("Received from native host:", message);
        // Process the message from the native app
        // e.g., update UI, store data, etc.
        if (message && message.Text) {
            console.log("Native host says:", message.Text);
        }
    });

    port.onDisconnect.addListener(() => {
        const lastError = chrome.runtime.lastError;
        if (lastError) {
            console.error("Native host disconnected with error:", lastError.message);
        } else {
            console.log("Native host disconnected.");
        }
        port = null; // Clear the port
        // Optionally, try to reconnect after a delay
        setTimeout(connectToNativeHost, 5000);
    });

    console.log("Native port connected (or connection attempt initiated).");
}

function sendMessageToNativeHost(message) {
    if (port) {
        console.log("Sending to native host:", message);
        port.postMessage(message);
    } else {
        console.error("Cannot send message: Native port is not connected.");
        // Optionally, try to connect first
        // connectToNativeHost();
        // setTimeout(() => sendMessageToNativeHost(message), 1000); // And then retry
    }
}

// --- Example Usage ---

// Connect when the extension starts (or on some event)
connectToNativeHost();

// Example: Send a message after a delay or on an event
setTimeout(() => {
    if (port) {
        sendMessageToNativeHost({ Text: "Hello from Chrome Extension!" });
    } else {
        console.warn("Port not yet ready for initial message.");
        // If you want to ensure it sends, you might queue this message
        // or wait for the port to be established.
        // For simplicity, we're just logging a warning here.
    }
}, 3000); // Wait 3 seconds for connection to establish

// Example: Listen for webNavigation events
chrome.webNavigation.onCompleted.addListener((details) => {
    if (details.frameId === 0) { // Main frame
        console.log("Navigated to:", details.url);
        if (port) {
            sendMessageToNativeHost({
                EventType: "Navigation",
                Url: details.url,
                Timestamp: new Date().toISOString()
            });
        } else {
            console.warn("Cannot send navigation event: Native port not connected.");
        }
    }
});

console.log("Background script loaded.");
