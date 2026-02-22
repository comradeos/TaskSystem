.PHONY: build up down clean logs rebuild migrate test

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

migrate:
	docker compose build --no-cache migrator
	docker compose --profile migrate run --rm migrator

test:
	docker run --rm \
		--network tasksystem_default \
		-v $(PWD)/api:/app \
		-w /app \
		mcr.microsoft.com/dotnet/sdk:8.0 \
		dotnet test Api.sln