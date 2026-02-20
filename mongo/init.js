function createUserIfNotExists(databaseName) {
    if (!databaseName) return;

    var appUser = process.env.MONGO_APP_USERNAME;
    var appPass = process.env.MONGO_APP_PASSWORD;

    if (!appUser || !appPass) {
        throw new Error("Missing MONGO_APP_USERNAME or MONGO_APP_PASSWORD");
    }

    var targetDb = db.getSiblingDB(databaseName);
    var existingUser = targetDb.getUser(appUser);

    if (!existingUser) {
        targetDb.createUser({
            user: appUser,
            pwd: appPass,
            roles: [
                { role: "readWrite", db: databaseName }
            ]
        });
        print("User created for DB: " + databaseName);
    } else {
        print("User already exists for DB: " + databaseName);
    }
}

createUserIfNotExists(process.env.MONGO_CORE_DATABASE);
createUserIfNotExists(process.env.MONGO_TIMELINE_DATABASE);