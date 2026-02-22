import { Link, useNavigate } from "react-router-dom"
import { useAuth } from "../auth/useAuth"
import { authApi } from "../api/auth.api"

function Header() {
    const { logout } = useAuth()
    const navigate = useNavigate()

    const handleLogout = async () => {
        try {
            await authApi.logout()
        } catch { }

        logout()
        navigate("/login")
    }

    return (
        <div className="header">
            <div className="header__nav">
                <Link to="/projects" className="header__nav-item">
                    Projects
                </Link>

                <Link to="/users" className="header__nav-item">
                    Users
                </Link>
            </div>

            <button
                className="button button--secondary"
                onClick={handleLogout}
            >
                Logout
            </button>
        </div>
    )
}

export default Header