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
			console.log(token);
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
				console.error(`Error fetching ${api_url} API: ${error}`);
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

function resetBrowserState(){
    chrome.storage.local.set({browserClosedNormally: false})
}

async function hadNormalClosure(){
    let normalClosure = (await chrome.storage.local.get(['browserClosedNormally'])).browserClosedNormally;
    if(normalClosure == null || normalClosure != true){
        console.log('Normal closure state: ' + normalClosure);
        console.log("Browser was not closed normally");
        return false;
    }else{
        console.log("Browser closed normally")
        return true;
    }
}

//Handles browser/extension being closed due to crashes/ power down etc.
//Basically, sends the last active browser time to the api for last activity, as on browser closure the last activity is not recorded.
async function manageUnwantedShutdown(){
    let normalClosure = await hadNormalClosure();
    //If browser/extension crashed/closed in a sudden way then you still have to record the last active browser time.
    //Otherwise, say your visit to a site A will be recorded as used for a looooong time.
    if(!normalClosure) console.log('Previous closure time: ' + await getLatestActiveTime());

    setInterval(setLatestActiveTime, 1000);

    resetBrowserState();
}