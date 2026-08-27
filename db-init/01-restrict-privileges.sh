#!/bin/sh
# Runs once, on first container init (only when the data directory is empty),
# via the official MariaDB image's /docker-entrypoint-initdb.d/ mechanism.
#
# Separates the two roles that previously shared the "news_service" MariaDB
# credential:
#   - news_service: runtime user for the long-running backend process.
#     DML only (no DDL), scoped to the praxis_news schema. If the backend
#     process is ever compromised, the attacker inherits data access, not
#     schema access.
#   - news_migrator: DDL user, used only for the explicit `--migrate` step.
#     Never held by the long-running app process.
#
# Table-level (rather than schema-level) grants for news_service would be
# tighter, but MariaDB requires the target table to already exist for a
# table-scoped GRANT — this script runs before any migration has created
# `news`, so it grants at the schema level instead.
#
# See docs/architecture/threat-model.md, "DB privilege separation".
set -eu

mariadb -uroot -p"${MARIADB_ROOT_PASSWORD}" <<-SQL
    REVOKE ALL PRIVILEGES, GRANT OPTION FROM 'news_service'@'%';
    GRANT SELECT, INSERT, UPDATE, DELETE ON \`praxis_news\`.* TO 'news_service'@'%';

    CREATE USER IF NOT EXISTS 'news_migrator'@'%' IDENTIFIED BY '${NEWS_MIGRATOR_PASSWORD}';
    GRANT CREATE, ALTER, DROP, INDEX, REFERENCES, SELECT, INSERT, UPDATE, DELETE
        ON \`praxis_news\`.* TO 'news_migrator'@'%';

    FLUSH PRIVILEGES;
SQL
