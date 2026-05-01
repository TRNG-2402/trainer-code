import React from 'react'
import { Link } from 'react-router-dom'

export default function NotFound() {
  return (
    <div>
        <h2>404 - Not Found</h2>
        <p>That page doesn't exist.</p>
        <Link to="/">Back home</Link>
    </div>
  )
}
