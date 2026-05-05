import { Link, useNavigate } from 'react-router-dom' // This gives us that <a> tag behavior but leveraging react routing
import styles from './NavBar.module.css' // Using a styling module for this component
import { useAuth } from '../context/AuthContext' // Importing my custom hook


export default function NavBar() {

  // Now that we have our Auth context setup, when a user logs in - the entire app can 
  // potentially see that info to do conditional rendering or update its views/ etc
  const { isAuthenticated, user, logout} = useAuth();
  const navigate = useNavigate();

  // Creating an event handler so that when someone clicks our button to logout
  // we call this handler and call that logout function... and probably redirect them too
  function handlelogout() {
    // When someone clicks our button, we log out via the logout function
    logout();
    // Navigate them to the login page (for now, maybe you make a confirmation page or something)
    navigate('/login');
  }

  return (
    <nav className={styles.css}>
        <Link to="/" className={styles.link}>Home</Link>
        <Link to="/products" className={styles.link}>Products</Link>
        <Link to="/categories" className={styles.link}>Categories</Link>
        
        { /* Based on whether a user is logged in (we check isAuthenticated for this) 
            we can render either the login link or a logout button*/
          isAuthenticated ? (
            <>
              <span className={styles.greeting}>Hello, {user?.username}</span>
              <button className={styles.linkButton} onClick={handlelogout}>
                Log out
              </button>
            </>
          ) : (
            <Link to="/login" className={styles.link}>Login</Link>
          )}
    </nav>
  );
}
