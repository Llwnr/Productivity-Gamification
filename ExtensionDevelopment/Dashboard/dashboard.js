document.addEventListener('DOMContentLoaded', function() {
    const content = document.getElementById('dashboard-content');
    content.innerHTML = '<p>Your dashboard data will appear here.</p>';

    const logoutButton = document.getElementById('logout-button');
    logoutButton.addEventListener('click', () => {
        chrome.storage.local.remove(['authToken'], () => {
            console.log('Logged out');
            window.close(); // Close the dashboard tab
        });
    });
});