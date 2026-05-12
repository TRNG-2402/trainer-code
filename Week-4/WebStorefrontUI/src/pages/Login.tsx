import React from 'react'
import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import axios from 'axios'
import { useAuth } from '../context/AuthContext'

export default function Login() {

  // This page will allow us to actually log into our site via the auth endpoints in our API
  // First, lets set some stuff up
  // We want to be able to redirect a user on successful login. We need, useNavigate()
  const navigate = useNavigate(); 
  const { login } = useAuth(); // grabbing that login function from our context, only ask for what we need

  // Similar to how we handled the search bar, we are going to useState to store user input
  // for our text inputs. We will store the string username and password the user enters
  // to attempt login in state, as well as any errors that come up 
  // if they do actually log in - the actual user info (jwt, userId, etc) is stored in context. 
  const [username, setUsername] = useState(''); 
  const [password, setPassword] = useState('');
  const [error, setError] = useState<string | null>(null);

  // Writing an event handler! We are going to use a form (our first HTML form inside react so far!)
  // that will have a submit button - we are writing our own logic for what actually happens when a user
  // clicks submit
  const handleSubmit = async (e: React.SubmitEvent) => {

    // Not unique to react OR TS - we do this in vanilla JS too
    // First thing we do inside of our event handler (for our submit form event)
    // is we prevent the default form behavior
    e.preventDefault(); // tell our browser "ignore the default form behavior"

    // Next, we want to validate user input 
    // If username or password are missing... just exit the function. We can't send the POST. 
    if(!username.trim() || !password.trim()) return;

    // We set our error state to null manually, on component mount. BUT what happens if the user fails 
    // to log in, triggers an error, and tries to log in again? We need to manually clear the error state
    // before we try again - so that any errors are "fresh" - otherwise any error conditional rendering
    // to alert the user will persist and our UI will be show stale information
    setError(null);

    try{
      // With a non-null username and password in hand, we will attempt login
      // Notice inside of AuthContext.tsx, the login function doesn't have a try catch
      // That's because we're catching any errors here. We also don't return anything here 
      // because login sets localStorage as well as state inside of AuthContext for us. 
      await login({username, password});
      navigate('/'); // If we log in successfully, redirect to the home page.

    } catch (err) {
      // If we get a 401 error, we know that our API will return something with a { status, message }
      // Surface this to the user in the UI - for now inside of this catch we do our part
      // by setting the error state with the message via setError
      //
      // STUDENT NOTE: We previously typed this as `catch (err: any)`, which tripped the
      // `@typescript-eslint/no-explicit-any` rule. `any` opts out of type checking entirely —
      // we lose the safety net TypeScript is supposed to give us. In modern TypeScript, the
      // type of a `catch` parameter is `unknown` by default, which forces us to NARROW the
      // error before we can access properties on it.
      //
      // Axios ships a built-in type guard, `axios.isAxiosError`, which narrows the unknown
      // error to `AxiosError` inside the `if` block. Once narrowed, TypeScript knows `.response`
      // exists and we can safely optional-chain into `.data.message`. This is the canonical
      // way to handle axios errors in a typesafe codebase.
      if (axios.isAxiosError(err)) {
        setError(err.response?.data?.message ?? 'Login failed');
      } else {
        setError('Login failed');
      }
    }
  };

  return (
    <form onSubmit={handleSubmit}>
      <h2>Login</h2>
      <div>
        <label>
          Username:{' '}
          <input 
            type="text"
            value={username}
            onChange={(e) => setUsername(e.target.value)}
          />
        </label>
      </div>
      <div>
        <label>
          Password:{' '}
          <input 
            type="password"
            value={password}
            onChange={(e) => setPassword(e.target.value)} 
          />
        </label>
      </div>
      {/* 
        Underneath our second div, but still inside the form, 
        we're going to use basic conditional render - if there is an error
        render a paragraph with the error contents in red. Just to alert the user.
      */}
      {error && <p style={{color: 'red'}}>{error}</p>}
      <button type='submit'>Log in</button>
    </form>
  )
}
