import { useEffect, useState } from "react"
import { usersApi } from "../api/users.api"
import { useAuth } from "../auth/useAuth"

function UsersPage() {

    const { user } = useAuth()

    const [users, setUsers] = useState([])
    const [loading, setLoading] = useState(true)

    const [form, setForm] = useState({
        name: "",
        login: "",
        password: "",
        isAdmin: false
    })

    const [saving, setSaving] = useState(false)
    const [error, setError] = useState("")

    useEffect(() => {
        loadUsers()
    }, [])

    const loadUsers = async () => {
        try {
            setLoading(true)
            const data = await usersApi.getAll()
            setUsers(data ?? [])
        } catch { } finally {
            setLoading(false)
        }
    }

    const handleCreate = async () => {
        if (!form.name || !form.login || !form.password) {
            setError("All fields are required")
            return
        }

        try {
            setSaving(true)
            setError("")

            await usersApi.create(form)

            setForm({
                name: "",
                login: "",
                password: "",
                isAdmin: false
            })

            loadUsers()
        } catch {
            setError("Failed to create user")
        } finally {
            setSaving(false)
        }
    }

    if (loading) return <div className="block">Loading...</div>

    return (
        <div className="block">

            <h1>Users</h1>

            {user?.isAdmin && (
                <div className="block mb-20">

                    <h3>Create User</h3>

                    <div className="block block--row">

                        <input
                            className="input"
                            placeholder="Name"
                            value={form.name}
                            onChange={(e) =>
                                setForm({ ...form, name: e.target.value })
                            }
                        />

                        <input
                            className="input"
                            placeholder="Login"
                            value={form.login}
                            onChange={(e) =>
                                setForm({ ...form, login: e.target.value })
                            }
                        />

                        <input
                            className="input"
                            type="password"
                            placeholder="Password"
                            value={form.password}
                            onChange={(e) =>
                                setForm({ ...form, password: e.target.value })
                            }
                        />

                        <label className="checkbox-inline">
                            <input
                                type="checkbox"
                                checked={form.isAdmin}
                                onChange={(e) =>
                                    setForm({ ...form, isAdmin: e.target.checked })
                                }
                            />
                            Admin
                        </label>

                        <button
                            className="button button--primary"
                            onClick={handleCreate}
                            disabled={saving}
                        >
                            {saving ? "Creating..." : "Create"}
                        </button>

                    </div>

                    {error && (
                        <div className="error mt-8">
                            {error}
                        </div>
                    )}

                </div>
            )}

            <table className="table">
                <thead>
                    <tr>
                        <th>ID</th>
                        <th>Name</th>
                    </tr>
                </thead>
                <tbody>
                    {users.map((u) => (
                        <tr key={u.id}>
                            <td>{u.id}</td>
                            <td>{u.name}</td>
                        </tr>
                    ))}
                </tbody>
            </table>

        </div>
    )
}

export default UsersPage