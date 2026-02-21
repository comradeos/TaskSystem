.PHONY: build up down clean logs rebuild migrate

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