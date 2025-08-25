console.log("Site Monitor content script loaded.");

// window.addEventListener('visibilitychange', () => {
//   if (document.visibilityState === 'visible') {
//     console.log("Tab is now visible. Sending 'tab_focused' message.");
//     // The tab has become visible (user returned to it)
//     chrome.runtime.sendMessage({ type: 'tab_focused' });
//   } else {
//     console.log("Tab is now hidden. Sending 'tab_blurred' message.");
//     // The tab has become hidden (user switched away)
//     chrome.runtime.sendMessage({ type: 'tab_blurred' });
//   }
// });