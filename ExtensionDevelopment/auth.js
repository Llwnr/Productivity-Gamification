const API_BASE = 'https://localhost:7131/Authentication/'
const loginForm = document.getElementById('login-form');

loginForm.addEventListener('submit', async(e)=>{
    e.preventDefault();

    const username = document.getElementById('username').value;
    const password = document.getElementById('password').value;

    const userLoginInfo = {
        username: username,
        password: password
    }

    await fetch(API_BASE + 'login', {
        method: 'POST',
        headers: {'Content-Type': 'application/json'},
        body: JSON.stringify(userLoginInfo)
    })
        .then(res => res.text().then(token => {
            chrome.storage.local.set({authToken: token});
        }))
        .catch(err => {console.log("Error while logging in: " + err)})
    
});