import { describe, it, expect, vi, beforeEach } from "vitest";
import { renderHook, act, waitFor } from "@testing-library/react";
import type { ReactNode } from "react";
import { AuthProvider, useAuth } from "./AuthContext";


// Our AuthProvider makes API calls using the authService  
// Because this is a unit test, we aren't trying to test whether or not we can connect to the API
// We just want to test the isolated functionality of the AuthProvider.
// Same logic as mocking a repo layer via Moq in xUnit
vi.mock('../services/authService', () => ({
    authService: {
        // We want to return a fake JWT where the payload decodes to an expected user. 
        // Within authService the thing that returns JWTs is the login function
        // Payload for this token should decode to 
        // nameid: '1', unique_name: 'alice', role: 'User, exp:99999999999
        login: vi.fn().mockResolvedValue({
            token:
                'eyJhbGciOiJIUzI1NiJ9.' +
                'eyJuYW1laWQiOiIxIiwidW5pcXVlX25hbWUiOiJhbGljZSIsInJvbGUiOiJVc2VyIiwiZXhwIjo5OTk5OTk5OTk5fQ.' +
                'sig',
            expiresAt: '2099-12-31T23:59:59Z'
        }),
    },
}));

// Because hooks need to be inside of a component, and because that component needs to be wrapped in
// a context provider - we create a wrapper function. Looks suspiciously like our AuthProvider. 
function wrapper( { children }: { children: ReactNode}) {
    return <AuthProvider>{children}</AuthProvider>
}

describe('useAuth', () => {

    // Because we are testing things that use login without logging our each and every time
    // We can write some function that needs to occur before each test.
    beforeEach(() => {
        // Before each test, flush local storage. 
        localStorage.clear();
    });

    it('starts logged out when localStorage is empty', () => {
        // We are going to render with renderHook, using our wrapper function
        // Looks a little different than when we just called render() and had it render
        // some JSX/TSX - but we can assert things via expect the same way
        const { result } = renderHook(() => useAuth(), { wrapper })

        expect(result.current.isAuthenticated).toBe(false);
        expect(result.current.user).toBeNull();
    });

    it('flips isAuthenticated flag boolean after login()', async () => {

        // Arrange
        const { result } = renderHook(() => useAuth(), { wrapper })

        // Act 
        await act(async () => {
            await result.current.login({ username: 'alice', password: 'secret'})
        });

        await waitFor(() => {
            expect(result.current.isAuthenticated).toBe(true);
            expect(result.current.user?.username).toBe('alice');
            expect(result.current.user?.role).toBe('User');
        });
    });

    it('clears state on logout()', async () => {

        // Arrange
        const { result } = renderHook(() => useAuth(), { wrapper })

        // Act 
        await act(async () => {
            await result.current.login({ username: 'alice', password: 'secret'})
        });

        act(() => {
            // Should reset state
            result.current.logout();
        })

        expect(result.current.isAuthenticated).toBe(false);
        expect(result.current.user).toBeNull();
        expect(localStorage.getItem('token')).toBeNull();

    });



});


