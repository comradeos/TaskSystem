include .env
export

.PHONY: build up down clean logs rebuild test

build:
	docker compose build

up:
	docker compose up -d

rebuild:
	docker compose build --no-cache
	docker compose up -d

down:
	docker compose down

clean:
	docker compose down -v --remove-orphans

logs:
	docker compose logs -f

test:
	docker run --rm \
		--network tasksystem_default \
		-v $(PWD)/api:/app \
		-w /app \
		mcr.microsoft.com/dotnet/sdk:8.0 \
		dotnet test TaskSystem.sln