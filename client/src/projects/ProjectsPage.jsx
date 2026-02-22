import { useEffect, useState } from "react"
import { useNavigate } from "react-router-dom"
import { projectsApi } from "../api/projects.api"

function ProjectsPage() {
    const [projects, setProjects] = useState([])
    const [loading, setLoading] = useState(true)
    const [newName, setNewName] = useState("")
    const [error, setError] = useState(null)

    const navigate = useNavigate()

    useEffect(() => {
        loadProjects()
    }, [])

    const loadProjects = async () => {
        try {
            const data = await projectsApi.getAll()
            setProjects(data)
        } catch (err) {
            console.error(err)
        } finally {
            setLoading(false)
        }
    }

    const handleCreate = async () => {
        if (!newName.trim()) return

        setError(null)

        try {
            await projectsApi.create(newName)
            setNewName("")
            loadProjects()
        } catch (err) {
            setError(err.message)
        }
    }

    return (
        <div className="block">
            <h1>Projects</h1>

            <div className="block block--row">
                <input
                    className="input"
                    placeholder="New project name"
                    value={newName}
                    onChange={(e) => {
                        setNewName(e.target.value)
                        setError(null)
                    }}
                />

                <button
                    className="button button--primary"
                    onClick={handleCreate}
                >
                    Create
                </button>
            </div>

            {error && (
                <div className="error">
                    {error}
                </div>
            )}

            {loading ? (
                <div>Loading...</div>
            ) : (
                <table className="table">
                    <thead>
                        <tr>
                            <th>ID</th>
                            <th>Name</th>
                        </tr>
                    </thead>
                    <tbody>
                        {projects.map((p) => (
                            <tr
                                key={p.id}
                                onClick={() => navigate(`/projects/${p.id}`)}
                            >
                                <td>{p.id}</td>
                                <td>{p.name}</td>
                            </tr>
                        ))}
                    </tbody>
                </table>
            )}
        </div>
    )
}

export default ProjectsPage