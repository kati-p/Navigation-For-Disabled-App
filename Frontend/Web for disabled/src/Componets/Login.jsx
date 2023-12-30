import React, { useState, useEffect } from 'react';
import styles from './Login.css';
import Axios from 'axios';
import { Snackbar } from '@material-ui/core';
import { Alert } from '@mui/material';
import { useCookies } from 'react-cookie';
import { useNavigate } from 'react-router-dom';


function Login() {

  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');

  const [snackbarOpen, setSnackbarOpen] = useState(false)
  const [snackbarMessage, setSnackbarMessage] = useState('');

  const [cookies, setCookie, removeCookie] = useCookies(['token'])

  const navigate = useNavigate()

  useEffect(() => {
    document.body.classList.remove('bg-main')
    document.body.classList.add('bg-login')
    removeCookie(['token'])
  },[])
  
  function handleLogin() {
  
    Axios.post('http://localhost:5022/tokens', {
          username : username,
          password : password,
      })
      .then(response => {
        // Handle the response
        const token = response.data !== null && response.data !== undefined ? response.data.token : '';
  
        setCookie('token', token)

        navigate('/main')
      })
      .catch (error => {
        
        setSnackbarMessage('Login failed. Please check your credentials.');
        setSnackbarOpen(true)
      }) 
  };
  
  function handleCloseSnackbar(){
    setSnackbarMessage('')
    setSnackbarOpen(false)
  };


  return (
    <div className={styles.login_page}>
        <div className="login-page">
          <div className="form">
            <div className="login">
              <div className="login-header">
                <h3>Login</h3>
              </div>
            </div>
            <div className="login-form">
              <input
                type="text"
                placeholder="username"
                value={username}
                onChange={(event) => setUsername(event.target.value)}
              />
              <input
                type="password"
                placeholder="password"
                value={password}
                onChange={(event) => setPassword(event.target.value)}
              />
              <button onClick={handleLogin}>Login</button>
              <p className="message">
                Don't have an account?{' '}
                <a href='' onClick={() => {navigate('/register')}}>Create new account</a>
              </p>
              <p className="contract">Got an issue ?  Give us a feedback at 6434428823@student.chula.ac.th</p>
            </div>
          </div>
        </div>

        <Snackbar
          open={snackbarOpen}
          autoHideDuration={5000}
          onClose={handleCloseSnackbar}
        >
          <Alert severity="error">
            {snackbarMessage}
          </Alert>
        </Snackbar>
    </div>
  );
}

export default Login;