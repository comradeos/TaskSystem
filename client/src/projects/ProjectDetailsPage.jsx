import { useEffect, useState } from "react"
import { useParams, useNavigate } from "react-router-dom"
import { tasksApi } from "../api/tasks.api"
import { usersApi } from "../api/users.api"
import { projectsApi } from "../api/projects.api"
import AddTaskModal from "../tasks/AddTaskModal"

function ProjectDetailsPage() {
    const { id } = useParams()
    const navigate = useNavigate()

    const [project, setProject] = useState(null)
    const [tasks, setTasks] = useState([])
    const [users, setUsers] = useState([])
    const [loading, setLoading] = useState(true)
    const [isAddModalOpen, setIsAddModalOpen] = useState(false)

    const [status, setStatus] = useState("")
    const [search, setSearch] = useState("")
    const [assignedUserId, setAssignedUserId] = useState("")
    const [page, setPage] = useState(1)
    const [size] = useState(10)

    useEffect(() => {
        loadProject()
        loadUsers()
    }, [id])

    useEffect(() => {
        loadTasks()
    }, [id, status, search, assignedUserId, page])

    const loadProject = async () => {
        try {
            const data = await projectsApi.getById(id)
            setProject(data)
        } catch (err) {
            console.error(err)
        }
    }

    const loadUsers = async () => {
        try {
            const data = await usersApi.getAll()
            setUsers(data)
        } catch (err) {
            console.error(err)
        }
    }

    const loadTasks = async () => {
        try {
            setLoading(true)

            const data = await tasksApi.getByProject(id, {
                page,
                size,
                status,
                search,
                assignedUserId
            })

            setTasks(data.items ?? data)
        } catch (err) {
            console.error(err)
        } finally {
            setLoading(false)
        }
    }

    const exportCsv = () => {
        const params = new URLSearchParams({
            page,
            size,
            status,
            search,
            assignedUserId,
            export: true
        }).toString()

        window.open(
            `http://localhost:5001/api/tasks/${id}?${params}`,
            "_blank"
        )
    }

    const formatDate = (dateString) => {
        return new Date(dateString).toLocaleString()
    }

    const getStatusName = (status) => {
        switch (status) {
            case 1: return "Todo"
            case 2: return "In Progress"
            case 3: return "Done"
            default: return "-"
        }
    }

    const getPriorityName = (priority) => {
        switch (priority) {
            case 1: return "Low"
            case 2: return "Medium"
            case 3: return "High"
            default: return "-"
        }
    }

    return (
        <div className="block">

            <button
                className="button"
                onClick={() => navigate("/projects")}
                style={{ marginBottom: 12 }}
            >
                All projects
            </button>

            <h1>
                {project?.name ?? `Project #${id}`}
            </h1>

            <div className="block block--row">

                <input
                    className="input"
                    placeholder="Search..."
                    value={search}
                    onChange={(e) => {
                        setSearch(e.target.value)
                        setPage(1)
                    }}
                />

                <select
                    className="input"
                    value={status}
                    onChange={(e) => {
                        setStatus(e.target.value)
                        setPage(1)
                    }}
                >
                    <option value="">All Statuses</option>
                    <option value="1">Todo</option>
                    <option value="2">In Progress</option>
                    <option value="3">Done</option>
                </select>

                <select
                    className="input"
                    value={assignedUserId}
                    onChange={(e) => {
                        setAssignedUserId(e.target.value)
                        setPage(1)
                    }}
                >
                    <option value="">All Assignees</option>
                    {users.map(u => (
                        <option key={u.id} value={u.id}>
                            {u.name}
                        </option>
                    ))}
                </select>

                <button className="button" onClick={exportCsv}>
                    Export CSV
                </button>

                <button
                    className="button"
                    onClick={() => navigate(`/projects/${id}/history`)}
                >
                    Show Project Timeline
                </button>

                <button
                    className="button button--primary"
                    onClick={() => setIsAddModalOpen(true)}
                >
                    Add Task
                </button>

            </div>

            {loading ? (
                <div>Loading...</div>
            ) : (
                <>
                    <table className="table">
                        <thead>
                            <tr>
                                <th>#</th>
                                <th>Title</th>
                                <th>Status</th>
                                <th>Priority</th>
                                <th>Author</th>
                                <th>Assignee</th>
                                <th>Created</th>
                            </tr>
                        </thead>
                        <tbody>
                            {tasks.map((t) => (
                                <tr
                                    key={t.id}
                                    onClick={() => navigate(`/tasks/${t.id}`)}
                                >
                                    <td>{t.numberInProject}</td>
                                    <td>{t.title}</td>
                                    <td>{getStatusName(t.status)}</td>
                                    <td>{getPriorityName(t.priority)}</td>
                                    <td>{t.authorUserName}</td>
                                    <td>{t.assignedUserName ?? "-"}</td>
                                    <td>{formatDate(t.createdAt)}</td>
                                </tr>
                            ))}
                        </tbody>
                    </table>

                    <div className="block block--row">
                        <button
                            className="button"
                            disabled={page <= 1}
                            onClick={() => setPage(p => p - 1)}
                        >
                            Prev
                        </button>

                        <div>Page {page}</div>

                        <button
                            className="button"
                            onClick={() => setPage(p => p + 1)}
                        >
                            Next
                        </button>
                    </div>
                </>
            )}

            {isAddModalOpen && (
                <AddTaskModal
                    projectId={id}
                    onClose={() => setIsAddModalOpen(false)}
                    onCreated={loadTasks}
                />
            )}
        </div>
    )
}

export default ProjectDetailsPage