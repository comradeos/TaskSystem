include .env
export

.PHONY: build up down clean logs rebuild profile

profile:
	@if [ "$(CORE_DB_PROVIDER)" = "mongo" ]; then \
		echo mongo; \
	elif [ "$(CORE_DB_PROVIDER)" = "postgres" ]; then \
		echo postgres; \
	else \
		echo "Unknown CORE_DB_PROVIDER=$(CORE_DB_PROVIDER)"; \
		exit 1; \
	fi

build:
	docker compose build

up:
	@PROFILE=$$(make -s profile) && \
	docker compose --profile $$PROFILE up -d

rebuild:
	@PROFILE=$$(make -s profile) && \
	docker compose --profile $$PROFILE build --no-cache && \
	docker compose --profile $$PROFILE up -d

down:
	docker compose down

clean:
	docker compose down -v --remove-orphans

logs:
	docker compose logs -f