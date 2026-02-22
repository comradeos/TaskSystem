import { useEffect, useState } from "react"
import { useParams, useNavigate, useLocation } from "react-router-dom"
import { timelineApi } from "../api/timeline.api"

function TimelinePage() {

    const { id } = useParams()
    const navigate = useNavigate()
    const location = useLocation()

    const [events, setEvents] = useState([])
    const [loading, setLoading] = useState(true)

    const isTask = location.pathname.includes("/tasks/")

    useEffect(() => {
        loadTimeline()
    }, [id])

    const loadTimeline = async () => {
        try {
            setLoading(true)

            const response = isTask
                ? await timelineApi.getTaskHistory(id)
                : await timelineApi.getProjectHistory(id)

            const data = response?.data ?? response ?? []

            setEvents(
                data.sort(
                    (a, b) =>
                        new Date(b.createdAt) - new Date(a.createdAt)
                )
            )
        } catch { } finally {
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

    const renderEventDetails = (event) => {
        const parsed = parseEventData(event.data)

        if (!parsed) return null

        switch (event.eventType) {

            case "ProjectCreated":
                return <div>Project name: <strong>{parsed.Name}</strong></div>

            case "TaskCreated":
                return (
                    <div>
                        <div>Title: <strong>{parsed.Title}</strong></div>
                        <div>Status: {parsed.Status}</div>
                        <div>Priority: {parsed.Priority}</div>
                        <div>Assigned User Id: {parsed.AssignedUserId ?? "None"}</div>
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
                        {parsed.Status && <div>Status → {parsed.Status}</div>}
                        {parsed.Priority && <div>Priority → {parsed.Priority}</div>}
                        {"AssignedUserId" in parsed && (
                            <div>Assigned → {parsed.AssignedUserId ?? "None"}</div>
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