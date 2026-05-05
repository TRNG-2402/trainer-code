import type { ReactNode } from "react"
import { Navigate, useLocation } from "react-router-dom"
import { useAuth } from "../context/AuthContext"

// Our route guard component will take in props so we need to type those
interface ProtectedRouteProps {
    children: ReactNode;
    requiredRole?: 'Admin' | 'User';
}

// In react, Route guards are JUST components
export default function ProtectedRoute({ children, requiredRole}: ProtectedRouteProps ) {
    
    // Ask for this info from the Auth context via our hook
    const { isAuthenticated, user } = useAuth();
    const location = useLocation();

    // Our guard will check different conditions and then return different JSX/TSX as needed
    if (!isAuthenticated) {
        // Navigate is a component based version of useNavigate - when it renders it sends the user
        // somewhere else (if that somewhere is a registered route)
        // 'replace' prevents the user from reloading the protected components after logging in
        // that kind of user behavior can break stuff if not accounted for
        // 'state' SHOULD let our login page bounce us back here after we successfully log in.
        return <Navigate to="/login" replace state={{ from: location}} />
    }

    // With this, we are setting up for admin and normal user views
    // First, check if the role is there at all. If not, we different issues
    // If it is, then make sure that the user's role matches the role passed into the guard.
    if(requiredRole && user?.role !== requiredRole) {
        // Autheticated but with the incorrect role
        // could render to some sort of /forbidden page 
        // for now we render an inline message
        return (
            <p style={{color: 'red'}}>
                You need the <strong>{requiredRole}</strong> role to view this page.
            </p>
        );
    }

    // But if we make it through all those checks and we satisfy the guard
    // all we will return are the components that are wrapped by the guard
    return <>{children}</>
}
