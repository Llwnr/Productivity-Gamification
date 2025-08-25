document.addEventListener('DOMContentLoaded', function() {
    const content = document.getElementById('dashboard-content');
    content.innerHTML = '<p>Your dashboard data will appear here.</p>';

    const logoutButton = document.getElementById('logout-button');
    logoutButton.addEventListener('click', async () => {
        await fetch('https://localhost:7131/Authentication/Logout', {
            method: 'POST',
            credentials: 'include'
        })
    });
});