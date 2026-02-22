import Header from "./Header"

function Layout({ children }) {
    return (
        <>
            <Header />
            <div className="page">
                <div className="page__container">
                    {children}
                </div>
            </div>
        </>
    )
}

export default Layout