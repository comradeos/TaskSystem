import { createContext, useState, useEffect } from "react"

export const AuthContext = createContext(null)

export function AuthProvider({ children }) {

    const [user, setUser] = useState(null)

    useEffect(() => {
        const stored = localStorage.getItem("user")

        if (!stored) return

        try {
            const parsed = JSON.parse(stored)

            if (parsed?.sessionToken) {
                setUser(parsed)
            } else {
                localStorage.removeItem("user")
                localStorage.removeItem("sessionToken")
            }
        } catch {
            localStorage.removeItem("user")
            localStorage.removeItem("sessionToken")
        }
    }, [])

    const login = (data) => {

        if (!data?.sessionToken) {
            console.error("Invalid login payload", data)
            return
        }

        const userData = {
            sessionToken: data.sessionToken,
            id: data.id,
            name: data.name,
            isAdmin: data.isAdmin
        }

        localStorage.setItem("user", JSON.stringify(userData))
        localStorage.setItem("sessionToken", data.sessionToken)

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