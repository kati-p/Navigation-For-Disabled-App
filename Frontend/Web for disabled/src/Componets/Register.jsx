import React, { useState, useEffect } from 'react';
import styles from './Register.css';
import Axios from 'axios';
import { Snackbar } from '@material-ui/core';
import { Alert } from '@mui/material';
import { useNavigate } from 'react-router-dom';
import { useCookies } from 'react-cookie';


function Register() {

  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');

  const [SnackbarMessage, setSnackbarMessage] = useState('');
  const [snackbarOpen, setSnackbarOpen] = useState(false);
  const [snackbarType, setSnackbarType] = useState("success");

  const [cookies, removeCookie] = useCookies(['token'])
  const navigate = useNavigate();

  useEffect(() => {
    document.body.classList.remove('bg-main')
    document.body.classList.add('bg-login')
    removeCookie(['token'])
  },[])
  
  function handleRegister() {

    if (!username || !password || !confirmPassword) {
      setSnackbarMessage('Please fill in all fields')
      setSnackbarOpen(true)
      setSnackbarType('error')
      return;
    }

    if (password !== confirmPassword) {
      setSnackbarMessage('Passwords do not match')
      setSnackbarOpen(true)
      setSnackbarType('error')
      return;
    }

    // Make the POST request to register
    Axios.post('http://localhost:5022/users', {
      username : username,
      password : password,
    })
    .then(response => {
      setSnackbarMessage('Registration successful!')
      setSnackbarOpen(true)
      setSnackbarType('success')
    })
    .catch(response => {
      setSnackbarMessage('Registration failed. Please try again.')
      setSnackbarOpen(true)
      setSnackbarType('error')
    })
};

function handleCloseSnackbar() {
  setSnackbarOpen(false)
  setSnackbarMessage('')
};

return (
  <>
    <div style={styles.register_page}>
      <div className="login-page">
        <div className="form">
          <div className="register">
            <div className="register-header">
              <h3>Registration</h3>
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
            <input
              type="password"
              placeholder="confirm password"
              value={confirmPassword}
              onChange={(event) => setConfirmPassword(event.target.value)}
            />
            <button onClick={handleRegister}>Sign Up</button>
            <p className="message">
              Already have an account? <a href='' onClick={() => { navigate('/login') }}>Login now</a>
            </p>
            <p className="contract">Got an issue ?  Give us a feedback at 6434428823@student.chula.ac.th</p>
          </div>
        </div>
      </div>

      {/* Snackbar to display registration success message */}
      <Snackbar
        open={snackbarOpen}
        autoHideDuration={5000}
        onClose={handleCloseSnackbar}
      >
        <Alert severity={snackbarType}>
          {SnackbarMessage}
        </Alert>
      </Snackbar>
    </div>
  </>
);
}

export default Register;