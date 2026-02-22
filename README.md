# TaskSystem

Minimal task management software to plan, organize and manage processes.

---

## Overview

TaskSystem is a clean architecture based task management system.

Backend: ASP.NET Core\
Database: PostgreSQL\
Timeline: MongoDB\
Frontend: React\
Containerization: Docker + Docker Compose

---

## Quick Start

### 1. Clone repository

``` bash
git clone git@github.com:comradeos/TaskSystem.git
cd TaskSystem
```

### 2. Configure environment

``` bash
cp ./.env.example ./.env
```

Edit `.env` if needed.

### 3. Build containers

``` bash
make build
```

### 4. Start services

``` bash
make up
```

### 5. Run migrations

``` bash
make migrate
```

------------------------------------------------------------------------

## Access

### Web Interface

    http://localhost:3001

Default credentials:

    login: admin
    password: admin

### API

    http://localhost:5001

---

## Development Commands

``` bash
make build      # build containers
make up         # start services
make down       # stop services
make migrate    # run migrations
make logs       # view logs
```
