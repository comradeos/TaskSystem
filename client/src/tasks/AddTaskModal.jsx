import { useEffect, useState } from "react"
import { usersApi } from "../api/users.api"
import { tasksApi } from "../api/tasks.api"

function AddTaskModal({ projectId, onClose, onCreated }) {
    const [title, setTitle] = useState("")
    const [description, setDescription] = useState("")
    const [status, setStatus] = useState("1")
    const [priority, setPriority] = useState("2")
    const [assignedUserId, setAssignedUserId] = useState("")

    const [users, setUsers] = useState([])
    const [error, setError] = useState(null)
    const [submitting, setSubmitting] = useState(false)

    useEffect(() => {
        loadUsers()
    }, [])

    const loadUsers = async () => {
        try {
            const data = await usersApi.getAll()
            setUsers(data)
        } catch (err) {
            console.error(err)
        }
    }

    const handleSubmit = async (e) => {
        e.preventDefault()

        if (!title.trim()) return

        setError(null)
        setSubmitting(true)

        try {
            await tasksApi.create({
                projectId: Number(projectId),
                title: title.trim(),
                description: description.trim(),
                status: Number(status),
                priority: Number(priority),
                assignedUserId: assignedUserId ? Number(assignedUserId) : null
            })

            onCreated()
            onClose()
        } catch (err) {
            setError(err.message)
        } finally {
            setSubmitting(false)
        }
    }

    return (
        <div className="modal-overlay" onClick={onClose}>
            <div className="modal" onClick={(e) => e.stopPropagation()}>
                <h2>Add Task</h2>

                <form className="form" onSubmit={handleSubmit}>

                    <div className="form__group">
                        <label className="label">Title</label>
                        <input
                            className="input"
                            value={title}
                            onChange={(e) => {
                                setTitle(e.target.value)
                                setError(null)
                            }}
                            required
                        />
                    </div>

                    <div className="form__group">
                        <label className="label">Description</label>
                        <textarea
                            className="textarea"
                            value={description}
                            onChange={(e) => {
                                setDescription(e.target.value)
                                setError(null)
                            }}
                        />
                    </div>

                    <div className="form__group">
                        <label className="label">Status</label>
                        <select
                            className="input"
                            value={status}
                            onChange={(e) => setStatus(e.target.value)}
                        >
                            <option value="1">Todo</option>
                            <option value="2">In Progress</option>
                        </select>
                    </div>

                    <div className="form__group">
                        <label className="label">Priority</label>
                        <select
                            className="input"
                            value={priority}
                            onChange={(e) => setPriority(e.target.value)}
                        >
                            <option value="1">Low</option>
                            <option value="2">Medium</option>
                            <option value="3">High</option>
                        </select>
                    </div>

                    <div className="form__group">
                        <label className="label">Assignee</label>
                        <select
                            className="input"
                            value={assignedUserId}
                            onChange={(e) => setAssignedUserId(e.target.value)}
                        >
                            <option value="">Unassigned</option>
                            {users.map(u => (
                                <option key={u.id} value={u.id}>
                                    {u.name}
                                </option>
                            ))}
                        </select>
                    </div>

                    {error && (
                        <div className="error">
                            {error}
                        </div>
                    )}

                    <div className="block block--row">
                        <button
                            className="button button--secondary"
                            type="button"
                            onClick={onClose}
                            disabled={submitting}
                        >
                            Cancel
                        </button>

                        <button
                            className="button button--primary"
                            type="submit"
                            disabled={submitting}
                        >
                            {submitting ? "Creating..." : "Create"}
                        </button>
                    </div>

                </form>
            </div>
        </div>
    )
}

export default AddTaskModal