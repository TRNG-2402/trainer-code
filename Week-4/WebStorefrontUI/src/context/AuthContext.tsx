import { createContext, useContext, useState, useMemo, type ReactNode } from "react";
import type { LoginRequest, AuthUser } from "../types/Auth";
import { authService } from "../services/authService";
import { jwtDecode } from "jwt-decode";


// The first thing we're going to define, is what the consumers of the useAuth() hook see
// What do they get by subscribing to our context?
interface AuthContextValue {
    token: string | null;
    user: AuthUser | null;
    isAuthenticated: boolean;
    login: (creds: LoginRequest) => Promise<void>; // Interface, so we provide method signature
    logout: () => void; // Context will provide login/logout functions
}

// Next, we create our context via the createContext function call
// we are going to provide a default value of undefined - because this will force any consumers of 
// context to be inside the provider tags (like BrowserRouter) - otherwise we'll get an error. 
// Providing some checking for us. 
const AuthContext = createContext<AuthContextValue | undefined>(undefined);

// Inside of our JWT are claim names. I goofed and did not tell ASP.NET to use the short names,
// such as "nameIdentifier", "name", etc - so we have the longform full URI's for the claim schemas
// http://schemas.microsoft.com/ws/2008/06/identity/claims/role - ugly. So we will hardcode their values 
// into some consts here, so we can avoid having to paste them all over and bloat out code

const NAMEID_URI = 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier';
const NAME_URI = 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name';
const ROLE_URI = 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role';

// Next, what does the payload of the decoded JWT look like? 
interface JwtPayload {
    nameid?: string;
    unique_name?: string;
    role?: 'User' | 'Admin';
    [NAMEID_URI]?: string;
    [NAME_URI]?: string;
    [ROLE_URI]?: 'User' | 'Admin';
    exp: number;
}

// Here we will use JWT-Decode to decode the token
function decodeUser(token: string): AuthUser {
    
    // payload should be of type JwtPayload - its the decoded values in the token
    const payload = jwtDecode<JwtPayload>(token);

    // Then we use those values to construct our AuthUser which is what we store in context
    // Multiple possible values for id - either we grab payload.nameid (shorthand name) 
    // we grab payload[NAMEID_URI] - longform name that im 90% sure we are using for this token
    // if neither of those work - set the value to 0 - we can do conditionals/error checking based on that
    const id = payload.nameid ?? payload[NAMEID_URI] ?? '0';
    const name = payload.unique_name ?? payload[NAME_URI] ?? '';
    const role = (payload.role ?? payload[ROLE_URI] ?? 'User') as 'User' | 'Admin';

    return {
        userId: Number(id),
        username: name,
        role
    }

}

// Now that we've created the functionality to decode our token, and pull out an AuthUser from it
// we will write the AuthProvider
export function AuthProvider ({ children }: { children: ReactNode}) {

    // So, we can think of a context as a way to centralize state. Contexts themselves
    // actually use state. And then anything in the provider gets access to the state
    // without having to worry about props. As long as they call that custom hook, 
    // that we will define below, and call useAuth().
    
    // Storing our token in State - notice we are going to store our token inside of localStorage
    // in lieu of actually implementing HttpCookies. In production you would NEVER EVER EVER do this
    // any script that runs on this page, say from a banner ad, can access local and session storage.
    // Again - not PRODUCTION SAFE

    // This will fire once - when we mount
    const [token, setToken] = useState<string | null>(() =>
        localStorage.getItem('token')
    );

    const [user, setUser] = useState<AuthUser | null>(() => {
        const t = localStorage.getItem('token'); // grabbing the token
        return t ? decodeUser(t) : null;
    });

    // Next thing we need, is a login function
    const login = async (creds: LoginRequest) => {
        
        //Using authService to send a login request to our backend
        const response = await authService.login(creds);

        // Setting an item in localStorage with the key 'token' and the value
        // of our JWT from the response
        localStorage.setItem('token', response.token); 

        // Calling our state setters for both setToken and setUser
        setToken(response.token);
        setUser(decodeUser(response.token));
    }

    // What happens (with regards to localStorage and state) when someone logs out?
    const logout = () => {
        localStorage.removeItem('token');
        setToken(null);
        setUser(null);
    }

    // We COULD do something called memoization via useMemo (another hook) to optimize our application
    // a little bit. Right now, without memo-izing anything, any time that someone logs in or out
    // we create a new object reference - AND every component wrapped by this provider re-renders
    // useMemo is pretty neat, because it tells React "don't throw out this object, just update its values"
    // and this can save on not just straight up memory (RAM use) but it can also save on re-renders 
    const value = useMemo<AuthContextValue>(
        () => ({
            token,
            user,
            isAuthenticated: token!== null,
            login,
            logout
        }),
        [token, user]
    );


    // Finally the return of Provider - it is JSX/TSX - and what we return are a react element
    // that FUNCTIONS the way that BrowserRouter does - doesn't render anything by itself
    // but PROVIDES information (state) to any children who render inside of the element tags
    return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;

}

// Finally, we need to provide a hook for child components to call if they need access
// to the state stored inside of AuthProvider. We will call ours useAuth()
export function useAuth(): AuthContextValue {
    const ctx = useContext(AuthContext);

    if (ctx === undefined) {
        throw new Error('useAuth hook MUST be used inside of an <AuthProvider>')
    }

    return ctx;
}
