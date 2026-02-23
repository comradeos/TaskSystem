# TaskSystem

Minimal task management software to plan, organize and manage processes.

---

## Overview

TaskSystem is a Clean Architecture based task management system.

Backend: ASP.NET Core 8\
Core Database: PostgreSQL\
Timeline Storage: MongoDB\
Frontend: React (Vite)\
Containerization: Docker + Docker Compose

---

## Quick Start

### 1. Clone Repository

If you use SSH:

``` bash
git clone git@github.com:comradeos/TaskSystem.git
cd TaskSystem
```

If you are not registered on GitHub or prefer HTTPS:

``` bash
git clone https://github.com/comradeos/TaskSystem.git
cd TaskSystem
```

---

### 2. Configure Environment

Linux / macOS:

``` bash
cp ./.env.example ./.env
```

Windows:

``` bash
copy .env.example .env
```

Edit `.env` if needed.

---

### 3. Build Containers

``` bash
make build
```

---

### 4. Start Services

``` bash
make up
```

---

### 5. Run Migrations

``` bash
make migrate
```

---

### 6. Run Tests

Linux / macOS:

``` bash
make test
```

Windows (without Make):

``` bash
make test_win
```

---

## Access

### Web Interface

http://localhost:3001

Default credentials:

login: admin\
password: admin

---

### API

http://localhost:5001

---

## Development Commands

``` bash
make build        # build containers
make up           # start services
make down         # stop services
make migrate      # run migrations
make logs         # view logs
make test         # run tests
make test_win     # run tests (windows)
```
