import { createContext, useState, useEffect } from "react"

export const AuthContext = createContext(null)

export function AuthProvider({ children }) {

    const [user, setUser] = useState(null)

    useEffect(() => {
        const stored = localStorage.getItem("user")
        if (stored) {
            setUser(JSON.parse(stored))
        }
    }, [])

    const login = (loginResponse) => {

        const userData = {
            sessionToken: loginResponse.sessionToken,
            id: loginResponse.id,
            name: loginResponse.name,
            isAdmin: loginResponse.isAdmin
        }

        localStorage.setItem("user", JSON.stringify(userData))
        localStorage.setItem("sessionToken", loginResponse.sessionToken)

        setUser(userData)
    }

    const logout = () => {
        localStorage.removeItem("user")
        localStorage.removeItem("sessionToken")
        setUser(null)
    }

    return (
        <AuthContext.Provider
            value={{
                user,
                token: user?.sessionToken,
                isAuthenticated: !!user,
                login,
                logout
            }}
        >
            {children}
        </AuthContext.Provider>
    )
}