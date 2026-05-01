import React from 'react'
import { Link } from 'react-router-dom' // This gives us that <a> tag behavior but leveraging react routing
import styles from './NavBar.module.css' // Using a styling module for this component

export default function NavBar() {
  return (
    <nav className={styles.css}>
        <Link to="/" className={styles.link}>Home</Link>
        <Link to="/products" className={styles.link}>Products</Link>
        <Link to="/categories" className={styles.link}>Categories</Link>
        <Link to="/login" className={styles.link}>Login</Link>
        <Link to="/login" className={styles.link}>Logout</Link>
    </nav>
  )
}
