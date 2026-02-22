import { useState } from "react"
import { useNavigate } from "react-router-dom"
import { useAuth } from "./useAuth"
import { authApi } from "../api/auth.api"

function LoginPage() {

    const [loginValue, setLoginValue] = useState("")
    const [password, setPassword] = useState("")
    const [error, setError] = useState(null)
    const [loading, setLoading] = useState(false)

    const navigate = useNavigate()
    const { login } = useAuth()

    const handleSubmit = async (e) => {
        e.preventDefault()

        if (loading) return

        setError(null)
        setLoading(true)

        try {
            const data = await authApi.login(loginValue, password)
            login(data)
            navigate("/projects", { replace: true })
        } catch (err) {
            setError(err.message || "Invalid login or password")
        } finally {
            setLoading(false)
        }
    }

    return (
        <div className="page">
            <div className="page__container">
                <div className="block block--center">
                    <h1>Login</h1>

                    <form className="form" onSubmit={handleSubmit}>
                        <div className="form__group">
                            <label className="label">Login</label>
                            <input
                                className="input"
                                value={loginValue}
                                onChange={(e) => setLoginValue(e.target.value)}
                                required
                            />
                        </div>

                        <div className="form__group">
                            <label className="label">Password</label>
                            <input
                                className="input"
                                type="password"
                                value={password}
                                onChange={(e) => setPassword(e.target.value)}
                                required
                            />
                        </div>

                        {error && (
                            <div className="error">
                                {error}
                            </div>
                        )}

                        <button
                            className="button button--primary"
                            type="submit"
                            disabled={loading}
                        >
                            {loading ? "Logging in..." : "Login"}
                        </button>
                    </form>
                </div>
            </div>
        </div>
    )
}

export default LoginPage