import { createContext, useState, useEffect } from "react"
import { STORAGE_SESSION_TOKEN_KEY, STORAGE_USER_KEY } from "../config"

export const AuthContext = createContext(null)

export function AuthProvider({ children }) {

    const [user, setUser] = useState(null)

    useEffect(() => {
        const stored = localStorage.getItem(STORAGE_USER_KEY)

        if (!stored) return

        try {
            const parsed = JSON.parse(stored)

            if (parsed?.sessionToken) {
                setUser(parsed)
            } else {
                localStorage.removeItem(STORAGE_USER_KEY)
                localStorage.removeItem(STORAGE_SESSION_TOKEN_KEY)
            }
        } catch {
            localStorage.removeItem(STORAGE_USER_KEY)
            localStorage.removeItem(STORAGE_SESSION_TOKEN_KEY)
        }
    }, [])

    const login = (data) => {

        if (!data?.sessionToken) return

        const userData = {
            sessionToken: data.sessionToken,
            id: data.id,
            name: data.name,
            isAdmin: data.isAdmin
        }

        localStorage.setItem(STORAGE_USER_KEY, JSON.stringify(userData))
        localStorage.setItem(STORAGE_SESSION_TOKEN_KEY, data.sessionToken)

        setUser(userData)
    }

    const logout = () => {
        localStorage.removeItem(STORAGE_USER_KEY)
        localStorage.removeItem(STORAGE_SESSION_TOKEN_KEY)
        setUser(null)
    }

    return (
        <AuthContext.Provider
            value={{
                user,
                token: user?.sessionToken ?? null,
                isAuthenticated: !!user?.sessionToken,
                login,
                logout
            }}
        >
            {children}
        </AuthContext.Provider>
    )
}