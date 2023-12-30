import React, { useState, useEffect } from 'react'
import { useNavigate } from 'react-router-dom'
import styles from "./Main.css";
import logo from './Picture/logo.png'
import user from './Picture/user.svg'
import axios from 'axios';
import { jwtDecode } from "jwt-decode";
import { Snackbar } from '@material-ui/core';
import { Alert } from '@mui/material';
import { useCookies } from 'react-cookie';


function Main() {

  const navigate = useNavigate();

  const [locations, setLocations] = useState([]);
  const [selectedSource, setSelectedSource] = useState('');
  const [selectedDestination, setSelectedDestination] = useState('');
  const [selectedLocation, setSelectedLocation] = useState('')

  const [username, setUsername] = useState('');

  const [SnackbarMessage, setSnackbarMessage] = useState('');
  const [snackbarOpen, setSnackbarOpen] = useState(false);
  const [snackbarType, setSnackbarType] = useState("success");

  const [data, setData] = useState({
    paths : [
      {
        path : [
          {
            bus : '',
            stations : [
              ''
            ]
          }
        ],
        weight : 0
      }
    ]
  });

  const [cookies, removeCookie] = useCookies(['token'])

  function handleCloseSnackbar() {
    setSnackbarOpen(false)
    setSnackbarMessage('')
  };


  useEffect(() => {

    document.body.classList.remove('bg-login')
    document.body.classList.add('bg-main')

    const token = cookies['token']
    //console.log(token)

    if (token != 'undefined') {
      try {
        const decodedToken = jwtDecode(token);
        setUsername(decodedToken.unique_name);
      } catch (error) {
        setSnackbarMessage('Error decoding token.')
        setSnackbarOpen(true)
        setSnackbarType('error')
      }

      // Use Axios to fetch data with the token in the headers
      axios
        .get('http://localhost:5022/stations', {
          headers: {
            Authorization: `Bearer ${token}`,
          },
        })
        .then(response => {
          const fetchedLocations = response.data;
          setLocations(fetchedLocations);
        })
        .catch(error => {
          setSnackbarMessage('Error fetching data.')
          setSnackbarOpen(true)
          setSnackbarType('error')
          console.log('Error fetching data:', error);
        });
    } else {
      setSnackbarMessage('Have not Token.')
      setSnackbarOpen(true)
      setSnackbarType('error')
      console.error('Have not Token.');
      setTimeout(() => {
        navigate('/login')
      }, 3000)
    }
  }, []);

  const [showSelection, setShowSelection] = useState(false);

  const handleStartClick = () => {
    

    const token = cookies['token']
    if (token != 'undefined') {

      console.log(selectedSource)
      console.log(selectedDestination)
      if (selectedSource === '' || selectedDestination === '') {
        setSnackbarMessage('Please Select Source and Destination.')
        setSnackbarOpen(true)
        setSnackbarType('error')
        console.log('Error fetching data:', error);
      }
      axios
        .post('http://localhost:5022/Paths', {
          "source": String(selectedSource),
          "destination": String(selectedDestination)
        }, {
          headers: {
            Authorization: `Bearer ${token}`,
          },
        })
        .then(response => {
          const fetchedData = response.data;
          console.log(fetchedData);
          setData(fetchedData);
          console.log(data);
          setShowSelection(true);
        })
        .catch(error => {
          setSnackbarMessage('Error fetching data.')
          setSnackbarOpen(true)
          setSnackbarType('error')
          console.log('Error fetching data:', error);
        });
    } else {
      setSnackbarMessage('Have not Token.')
      setSnackbarOpen(true)
      setSnackbarType('error')
      console.error('Have not Token.');
      setTimeout(() => {
        navigate('/login')
      }, 3000)
    }
  };

  const handleEndClick = () => {
    // Reset the selected values and hide the selection
    setSelectedDestination('');
    setSelectedSource('');
    setShowSelection(false);
    if (showSelection) {
      setSnackbarMessage('Thank you for using the service.')
      setSnackbarOpen(true)
      setSnackbarType('success')
    }
    setData({
      paths : [
        {
          path : [
            {
              bus : '',
              stations : [
                ''
              ]
            }
          ],
          weight : 0
        }
      ]
    });
  };

  function logout() {
    removeCookie(['token'])
    navigate('/login')
  }

  return (
    <div className={styles.main_page}>
      <header>
        <img className="logo" src={logo} alt='logo' />
        <button className='logout' onClick={logout}>Log out</button>
        <img className='user' src={user} alt='user' />
        <div className='username'>
          {username}
        </div>
      </header>
      <div>
        <div className='from'>
          <label className='custom-select' htmlFor="From">From</label><br></br>
          <select
            name="station"
            id="From"
            value={selectedSource}
            onChange={(e) => {
              setSelectedSource(e.target.value)
            }}
          >
            <option value="">Select location</option>
            {locations.map((location, index) => (
              <option key={index} value={location.station_ID}>
                {location.station_name}
              </option>
            ))}
          </select>
        </div>

        <div className='final'>
          <label className='custom-select2' htmlFor="destination">Destination</label><br></br>
          <select
            name="station"
            id="destination"
            value={selectedDestination}
            onChange={(e) => {
              setSelectedDestination(e.target.value)
            }}
          >
            <option value="">Select Destination</option>
            {locations.map((location, index) => (
              <option key={index} value={location.station_ID}>
                {location.station_name}
              </option>
            ))}
          </select>
        </div>

        <button className='start' onClick={handleStartClick} >Start</button>
        <button className='end' onClick={handleEndClick}>End</button>
        {showSelection && (
          <div className="box">
            <div className="rectangle">
              <div className='data'>
              {data.paths.map((path, index) => (
              <div key={index}>
                <h3>เส้นทางที่ {index + 1}</h3>
                <p>ระยะทาง {path.weight * 100}</p>
                {path.path.map((busPath, pathIndex) => (
                  <div key={pathIndex}>
                    <p>ขึ้นรถ {busPath.bus}</p>
                    <ul>ผ่านเส้นทาง</ul>
                    {busPath.stations.map((station, stationIndex) => (
                      <li key={stationIndex}>{station}</li>
                    ))}
                  </div>
                ))}
                </div>
              
            ))}
              </div>
            </div>
          </div>
        )}


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
    </div>
  )
}

export default Main