import { useEffect, useMemo, useState } from "react"
import { useParams, useNavigate, useLocation } from "react-router-dom"
import { timelineApi } from "../api/timeline.api"

function TimelinePage() {

    const { id } = useParams()
    const navigate = useNavigate()
    const location = useLocation()

    const [events, setEvents] = useState([])
    const [loading, setLoading] = useState(true)
    const [users, setUsers] = useState([])
    const [usersLoading, setUsersLoading] = useState(true)

    const isTask = location.pathname.includes("/tasks/")

    const statusMap = useMemo(() => ({
        1: "Todo",
        2: "InProgress",
        3: "Done"
    }), [])

    const priorityMap = useMemo(() => ({
        1: "Low",
        2: "Medium",
        3: "High"
    }), [])

    const usersById = useMemo(() => {
        const map = {}
        for (const u of users) map[u.id] = u.name
        return map
    }, [users])

    useEffect(() => {
        loadUsers()
    }, [])

    useEffect(() => {
        loadTimeline()
    }, [id, isTask])

    const loadUsers = async () => {
        try {
            setUsersLoading(true)

            const response = await fetch("http://localhost:5001/api/users")
            const json = await response.json()

            const data = json?.data ?? []
            setUsers(Array.isArray(data) ? data : [])
        } catch {
            setUsers([])
        } finally {
            setUsersLoading(false)
        }
    }

    const loadTimeline = async () => {
        try {
            setLoading(true)

            const response = isTask
                ? await timelineApi.getTaskHistory(id)
                : await timelineApi.getProjectHistory(id)

            const data = response?.data ?? response ?? []

            setEvents(
                (Array.isArray(data) ? data : []).sort(
                    (a, b) => new Date(b.createdAt) - new Date(a.createdAt)
                )
            )
        } catch {
            setEvents([])
        } finally {
            setLoading(false)
        }
    }

    const formatDate = (dateString) => {
        const date = new Date(dateString)
        return isNaN(date) ? "-" : date.toLocaleString()
    }

    const parseEventData = (raw) => {
        if (!raw) return null
        try {
            return JSON.parse(raw)
        } catch {
            return null
        }
    }

    const formatStatus = (value) => {
        if (value == null) return "-"
        return statusMap[value] ?? String(value)
    }

    const formatPriority = (value) => {
        if (value == null) return "-"
        return priorityMap[value] ?? String(value)
    }

    const formatUser = (idValue) => {
        if (idValue == null) return "None"
        const name = usersById[idValue]
        if (name) return name
        return `User #${idValue}`
    }

    const renderEventDetails = (event) => {
        const parsed = parseEventData(event.data)
        if (!parsed) return null

        switch (event.eventType) {

            case "ProjectCreated":
                return (
                    <div>
                        Project name: <strong>{parsed.Name}</strong>
                    </div>
                )

            case "TaskCreated":
                return (
                    <div>
                        <div>Title: <strong>{parsed.Title}</strong></div>
                        <div>Status: {formatStatus(parsed.Status)}</div>
                        <div>Priority: {formatPriority(parsed.Priority)}</div>
                        <div>Assigned: {formatUser(parsed.AssignedUserId)}</div>
                    </div>
                )

            case "CommentAdded":
                return (
                    <div>
                        Comment: <strong>{parsed.Content}</strong>
                    </div>
                )

            case "TaskUpdated":
                return (
                    <div>
                        {"Status" in parsed && (
                            <div>Status: {formatStatus(parsed.Status)}</div>
                        )}

                        {"Priority" in parsed && (
                            <div>Priority: {formatPriority(parsed.Priority)}</div>
                        )}

                        {"AssignedUserId" in parsed && (
                            <div>Assigned: {formatUser(parsed.AssignedUserId)}</div>
                        )}
                    </div>
                )

            default:
                return (
                    <pre className="pre">
                        {JSON.stringify(parsed, null, 2)}
                    </pre>
                )
        }
    }

    if (loading) return <div className="block">Loading...</div>

    return (
        <div className="block">

            <button
                className="button mb-12"
                onClick={() => navigate(-1)}
            >
                Back
            </button>

            <h1>
                {isTask ? "Task Timeline" : "Project Timeline"}
            </h1>

            <div className="block">

                {usersLoading && (
                    <div className="mb-12">Loading users...</div>
                )}

                {events.length === 0 && (
                    <div>No history yet</div>
                )}

                {events.map((e, index) => (
                    <div
                        key={`${e.createdAt}-${index}`}
                        className="timeline-item"
                    >
                        <div className="timeline-item__title">
                            {e.userName} — {e.eventType} — {formatDate(e.createdAt)}
                        </div>

                        <div className="timeline-item__body">
                            {renderEventDetails(e)}
                        </div>
                    </div>
                ))}

            </div>

        </div>
    )
}

export default TimelinePage