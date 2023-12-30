import { useState } from 'react'
import reactLogo from './assets/react.svg'
import viteLogo from '/vite.svg'
import './App.css'
import { Routes, Route } from 'react-router-dom'
import Login from './Componets/Login'
import Main from './Componets/Main'
import Register from './Componets/Register'

function App() {

  return (
    <Routes>
      <Route path="/" element={<Login/>}/>
      <Route path="/login" element={<Login/>}/>
      <Route path="/register" element={<Register/>}/>
      <Route path="/main" element={<Main/>}/>
    </Routes>
  )
}

export default App
