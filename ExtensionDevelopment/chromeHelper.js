const API_BASE_URL = "https://localhost:7131/SiteMonitor";
const API_ENDPOINT = "AnalyzeSite";

const delay = (durationMs) => {
  return new Promise(resolve => setTimeout(resolve, durationMs));
}

function setLatestActiveTime(){
    chrome.storage.local.set({latestActiveTime: new Date().toUTCString()});
}

async function getLatestActiveTime(){
    let activeTime = (await chrome.storage.local.get(['latestActiveTime'])).latestActiveTime;
    return activeTime;
}

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
    return (await chrome.storage.local.get(['authToken'])).authToken;
}

function clearActiveTabCache(){
    activeTabId = null;
    activeFullUrl = null;
}

var siteAnalysisDebouncer = 99999999; 
async function setSiteVisited(url, tabId, triggerType) {
    let token = await getIdToken();
    if(token == null){
        console.log("Token is null");
        return;
    }
    clearTimeout(siteAnalysisDebouncer); //Cancel any short lived activity
    if(!url) return;
    //Check if tab has been switched to a non browsable url i.e. browser setting pages etc
    if(triggerType == 'tabSwitched' && !(url.startsWith('http://') || url.startsWith('https://'))){
        await notifyBrowsingStopped();
        return;
    }

	//Check if its a valid browsable url and not the browser's settings stuff
	if (!url.startsWith('http://') && !url.startsWith('https://')) return;
	if (activeFullUrl == url && activeTabId == tabId) return; //Don't do anything if same page and same tab

	
	// let urlWithDetail = tabId + " " + triggerType + " " + url;
    siteAnalysisDebouncer = setTimeout(()=>{
		getTitleAndDescription(tabId, (tags) => {
			const title = (tags.title == null || tags.title.length > 0) ? tags.title.substring(0, 100) : "null";
			const desc = tags.description || "";
            const requestData = {
                url: url,
                title: title,
                description: desc
            };
            sendMessage("Calling for analysis of: " + title);
            const api_url = `${API_BASE_URL}/${API_ENDPOINT}`;
			fetch(api_url, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${token}`
                },
                body: JSON.stringify(requestData)
            })
			.then(response => {
				if (!response.ok) {
					console.error(`API call failed with status: ${response.status}\nUrl:${api_url}`);
				}
			})
			.catch(error => {
				console.error(`Error fetching ${api_url} API: ${error}`);
			});
		})
		},3000);

	activeTabId = tabId;
	activeFullUrl = url
}

async function notifyBrowsingStopped(){
    let notifyBrowsingStopped = `${API_BASE_URL}/BrowsingStopped`;
    let token = await getIdToken();
    fetch(notifyBrowsingStopped, {
        method: 'GET',
        headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${token}`
        }
    })

    clearActiveTabCache();
}

//Handles browser/extension being closed due to crashes/ power down etc.
//Basically, sends the last active browser time to the api for last activity, as on browser closure the last activity is not recorded.
async function notifyLastActiveTime(){
    let lastActiveTime = await getLatestActiveTime();
    console.log('Previous closure time: ' + lastActiveTime);

    chrome.storage.local.set({browserClosedNormally: false});

    let inactivityNotifyingApi = `${API_BASE_URL}/BrowserCrashed?lastActiveTimeStr=${encodeURIComponent(lastActiveTime)}`;
    let token = await getIdToken();
    fetch(inactivityNotifyingApi, {
        method: 'GET',
        headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${token}`
        }
    }).catch((err) => console.log("Error notifying inactivity state: " + err));
}

function isTokenExpired(token) {
  const expiry = (JSON.parse(atob(token.split('.')[1]))).exp;
  return (Math.floor((new Date()).getTime() / 1000)) >= expiry;
}

async function sendMessage(msg){
    let msgApi = `${API_BASE_URL}/Talk?msg=${encodeURIComponent(msg)}`;
    fetch(msgApi)
}