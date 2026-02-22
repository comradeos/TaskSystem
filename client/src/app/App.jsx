import { Routes, Route, Navigate } from "react-router-dom"
import { useAuth } from "../auth/useAuth"

import LoginPage from "../auth/LoginPage"
import ProjectsPage from "../projects/ProjectsPage"
import ProjectDetailsPage from "../projects/ProjectDetailsPage"
import TaskDetailsPage from "../tasks/TaskDetailsPage"
import ProtectedRoute from "../components/ProtectedRoute"
import Layout from "../components/Layout"
import UsersPage from "../users/UsersPage"
import TimelinePage from "../timeline/TimelinePage"

function App() {
  const { isAuthenticated } = useAuth()

  return (
    <Routes>

      <Route
        path="/login"
        element={
          isAuthenticated
            ? <Navigate to="/projects" />
            : <LoginPage />
        }
      />

      <Route
        path="/projects"
        element={
          <ProtectedRoute>
            <Layout>
              <ProjectsPage />
            </Layout>
          </ProtectedRoute>
        }
      />

      <Route
        path="/projects/:id"
        element={
          <ProtectedRoute>
            <Layout>
              <ProjectDetailsPage />
            </Layout>
          </ProtectedRoute>
        }
      />

      <Route
        path="/tasks/:id"
        element={
          <ProtectedRoute>
            <Layout>
              <TaskDetailsPage />
            </Layout>
          </ProtectedRoute>
        }
      />

      <Route
        path="/users"
        element={
          <ProtectedRoute>
            <Layout>
              <UsersPage />
            </Layout>
          </ProtectedRoute>
        }
      />

      <Route path="*" element={<Navigate to="/projects" />} />

      <Route
        path="/tasks/:id/history"
        element={
          <ProtectedRoute>
            <Layout>
              <TimelinePage />
            </Layout>
          </ProtectedRoute>
        }
      />

      <Route
        path="/projects/:id/history"
        element={
          <ProtectedRoute>
            <Layout>
              <TimelinePage />
            </Layout>
          </ProtectedRoute>
        }
      />

    </Routes>
  )
}

export default App