import { useEffect, useState } from "react"
import { useParams, useNavigate } from "react-router-dom"
import { tasksApi } from "../api/tasks.api"
import { commentsApi } from "../api/comments.api"
import { usersApi } from "../api/users.api"

function TaskDetailsPage() {
    const { id } = useParams()
    const navigate = useNavigate()

    const [task, setTask] = useState(null)
    const [users, setUsers] = useState([])
    const [comments, setComments] = useState([])
    const [newComment, setNewComment] = useState("")
    const [loading, setLoading] = useState(true)

    const [form, setForm] = useState({
        status: 1,
        priority: 1,
        assignedUserId: ""
    })

    const [saveStatus, setSaveStatus] = useState(null)

    useEffect(() => {
        loadData()
    }, [id])

    const loadData = async () => {
        try {
            setLoading(true)

            const taskData = await tasksApi.getById(id)
            const commentsData = await commentsApi.getByTask(id)
            const usersData = await usersApi.getAll()

            setTask(taskData)

            setForm({
                status: taskData.status,
                priority: taskData.priority,
                assignedUserId: taskData.assignedUserId || ""
            })

            setComments(
                (commentsData ?? [])
                    .sort((a, b) =>
                        new Date(b.createdAt) - new Date(a.createdAt)
                    )
            )

            setUsers(usersData ?? [])
        } catch (err) {
            console.error(err)
        } finally {
            setLoading(false)
        }
    }

    const handleSave = async () => {
        try {
            setSaveStatus("saving")

            const payload = {
                status: form.status,
                priority: form.priority,
                assignedUserId:
                    form.assignedUserId === ""
                        ? null
                        : Number(form.assignedUserId)
            }

            const updated = await tasksApi.update(id, payload)

            setTask(updated)
            setSaveStatus("saved")

            setTimeout(() => {
                setSaveStatus(null)
            }, 2000)

        } catch (err) {
            console.error(err)
            setSaveStatus("error")
        }
    }

    const handleAddComment = async () => {
        if (!newComment.trim()) return

        try {
            await commentsApi.create({
                taskId: Number(id),
                content: newComment.trim()
            })

            setNewComment("")
            loadData()
        } catch (err) {
            console.error(err)
        }
    }

    const formatDate = (dateString) => {
        if (!dateString) return "-"
        const date = new Date(dateString)
        if (isNaN(date)) return "-"
        return date.toLocaleString()
    }

    if (loading) return <div className="block">Loading...</div>
    if (!task) return <div className="block">Task not found</div>

    return (
        <div className="block">

            <button
                className="button"
                onClick={() => navigate(-1)}
            >
                Back
            </button>

            <h1>
                Task #{task.numberInProject ?? "-"}
            </h1>

            <div className="block">

                <div><strong>Title:</strong> {task.title || "-"}</div>

                {task.description && (
                    <div>
                        <strong>Description:</strong> {task.description}
                    </div>
                )}

                <div><strong>Author:</strong> {task.authorUserName || "-"}</div>

                <div>
                    <strong>Created:</strong> {formatDate(task.createdAt)}
                </div>

                <div className="form__group">
                    <label>Status</label>
                    <select
                        className="input"
                        value={form.status}
                        onChange={(e) =>
                            setForm({
                                ...form,
                                status: Number(e.target.value)
                            })
                        }
                    >
                        <option value="1">Todo</option>
                        <option value="2">In Progress</option>
                        <option value="3">Done</option>
                    </select>
                </div>

                <div className="form__group">
                    <label>Priority</label>
                    <select
                        className="input"
                        value={form.priority}
                        onChange={(e) =>
                            setForm({
                                ...form,
                                priority: Number(e.target.value)
                            })
                        }
                    >
                        <option value="1">Low</option>
                        <option value="2">Medium</option>
                        <option value="3">High</option>
                    </select>
                </div>

                <div className="form__group">
                    <label>Assignee</label>
                    <select
                        className="input"
                        value={form.assignedUserId}
                        onChange={(e) =>
                            setForm({
                                ...form,
                                assignedUserId: e.target.value
                            })
                        }
                    >
                        <option value="">Unassigned</option>
                        {users.map(u => (
                            <option key={u.id} value={u.id}>
                                {u.name}
                            </option>
                        ))}
                    </select>
                </div>

                <button
                    className="button button--primary"
                    onClick={handleSave}
                >
                    Save
                </button>

                {saveStatus === "saving" && (
                    <div style={{ marginTop: 8 }}>Saving...</div>
                )}

                {saveStatus === "saved" && (
                    <div style={{ marginTop: 8, color: "green" }}>
                        Saved
                    </div>
                )}

                {saveStatus === "error" && (
                    <div style={{ marginTop: 8, color: "red" }}>
                        Error while saving
                    </div>
                )}

                <button
                    className="button"
                    onClick={() => navigate(`/tasks/${id}/history`)}
                >
                    Show Timeline
                </button>

            </div>

            <hr />

            <h2>Comments</h2>

            <div className="block">
                <textarea
                    className="textarea"
                    value={newComment}
                    onChange={(e) => setNewComment(e.target.value)}
                />

                <button
                    className="button button--primary"
                    onClick={handleAddComment}
                >
                    Add Comment
                </button>
            </div>

            <div className="block">
                {comments.length === 0 && (
                    <div>No comments yet</div>
                )}

                {comments.map(c => (
                    <div key={c.id} className="block">
                        <div>
                            <strong>{c.userName}</strong>
                            {" — "}
                            {formatDate(c.createdAt)}
                        </div>
                        <div>{c.content}</div>
                    </div>
                ))}
            </div>

        </div>
    )
}

export default TaskDetailsPage