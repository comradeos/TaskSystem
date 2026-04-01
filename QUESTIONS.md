# SQL Задачі

## Задача №1

Існує таблиця з даними, в якій існують рядки, що повторюються, за значеннями деяких стовпців. Необхідно отримати список рядків, що повторюються. Потім видалити рядки, що повторюються, залишивши тільки по одному унікальному  рядку (залишити тільки найпізніші рядки).

По можливості навести кілька варіантів рішень.

## Підготовка

### Створення таблиці

```sql
CREATE TABLE employees (
    id SERIAL PRIMARY KEY,
    name TEXT NOT NULL,
    salary INT NOT NULL,
    department TEXT NOT NULL
);
```

### Наповнення таблиці

```sql
INSERT INTO employees (name, salary, department) VALUES
('Ivan', 1000, 'IT'),
('Ivan', 1000, 'IT'),
('Ivan', 1000, 'IT'),
('Ivan', 1000, 'HR'),
('Olena', 2000, 'Finance'),
('Olena', 2000, 'Finance'),
('Petro', 1500, 'IT');
```

## Отримати список рядків, що повторюються

### Варіант 1 (GROUP BY)

### Запит

```sql
SELECT 
    name,
    salary,
    department,
    -- кількість рядків в кожній групі
    COUNT(*) AS counter
FROM employees
-- групуємо за name salary та department
GROUP BY
    name,
    salary,
    department
-- де більше одного запису
HAVING COUNT(*) > 1;
```

### Результат

| name  | salary | department | counter |
|-------|--------|------------|---------|
| Olena | 2000   | Finance    | 2       |
| Ivan  | 1000   | IT         | 3       |


### Варіант 2 (IN)

### Запит

```sql
SELECT *
FROM employees
-- перевіряємо чи комбінація значень входить у список дублікатів
WHERE (name, salary, department) IN (
    -- внутрішній запит повертає комбінації значень
    SELECT name, salary, department
    FROM employees 
    -- групуємо за name + salary + department
    GROUP BY name, salary, department
    -- залишаємо тільки ті які повторюються більше одного разу
    HAVING COUNT(*) > 1.
) 
-- сортуємо
ORDER BY name, salary, department, id;
```

### Результат

| id | name  | salary | department |
|----|-------|--------|------------|
| 1  | Ivan  | 1000   | IT         |
| 2  | Ivan  | 1000   | IT         |
| 3  | Ivan  | 1000   | IT         |
| 5  | Olena | 2000   | Finance    |
| 6  | Olena | 2000   | Finance    |

### Варіант 3 (JOIN)

### Запит

```sql
SELECT e.*
FROM employees e
     -- приєднуємо результат підзапиту
    JOIN (
        -- підзапит повертає те шо повторюється
        SELECT name, salary, department    
        FROM employees
        GROUP BY name, salary, department
        HAVING COUNT(*) > 1
    ) d
-- приєднання по name + salary + department
ON e.name = d.name                     
AND e.salary = d.salary
AND e.department = d.department
ORDER BY e.name, e.salary, e.department, e.id;
```

### Результат

| id | name  | salary | department |
|----|-------|--------|------------|
| 1  | Ivan  | 1000   | IT         |
| 2  | Ivan  | 1000   | IT         |
| 3  | Ivan  | 1000   | IT         |
| 5  | Olena | 2000   | Finance    |
| 6  | Olena | 2000   | Finance    |

### Варіант 3 (EXISTS)

### Запит

```sql
SELECT e1.*
FROM employees e1
-- перевіряємо чи існує інший рядок з такими ж значеннями
WHERE EXISTS (
    -- повертаємо 1 бо не важливо що саме а вожливо саме існування
    SELECT 1
    -- знову звертаємось до тієї ж таблиці
    FROM employees e2
    -- збігаємо по name + salary + department
    WHERE e1.name = e2.name
      AND e1.salary = e2.salary
      AND e1.department = e2.department
      -- але не збігаємо по id
      AND e1.id <> e2.id
)
ORDER BY e1.name, e1.salary, e1.department, e1.id;
```

### Результат

| id | name  | salary | department |
|----|-------|--------|------------|
| 1  | Ivan  | 1000   | IT         |
| 2  | Ivan  | 1000   | IT         |
| 3  | Ivan  | 1000   | IT         |
| 5  | Olena | 2000   | Finance    |
| 6  | Olena | 2000   | Finance    |

## Видалити рядки, що повторюються
(залишивши тільки по одному унікальному  рядку залишити тільки найпізніші рядки)

### Варіант 1 (ROW_NUMBER)

### Запит

```sql
DELETE FROM employees
-- видаляємо ті шо потраплять у підзапит
WHERE id IN (
    SELECT id
        FROM (
            -- створюємо промежуточний результат
            SELECT
            id,
            -- порядковий номер у межах групи
            ROW_NUMBER() OVER (
                -- групуємо по комбінації колонок
                PARTITION BY name, salary, department
                -- сортуємо так шоб останній (найбільший) id був першим
                ORDER BY id DESC
            ) AS rn
            FROM employees
        ) t
    -- залишаємо тільки старі рядкти
    WHERE rn > 1                               
);
```

```sql
SELECT * FROM employees;
```

### Результат

| id | name  | salary | department |
|----|-------|--------|------------|
| 3  | Ivan  | 1000   | IT         |
| 4  | Ivan  | 1000   | HR         |
| 6  | Olena | 2000   | Finance    |
| 7  | Petro | 1500   | IT         |

### Варіант 2 (DELETE USING)

### Запит

```sql
DELETE FROM employees e1
USING employees e2
-- співпадає name salary department
WHERE e1.name = e2.name                       
  AND e1.salary = e2.salary
  AND e1.department = e2.department
  -- але id менший (старіший рядок)
  AND e1.id < e2.id;
```

```sql
SELECT * FROM employees;
```

### Результат

| id | name  | salary | department |
|----|-------|--------|------------|
| 3  | Ivan  | 1000   | IT         |
| 4  | Ivan  | 1000   | HR         |
| 6  | Olena | 2000   | Finance    |
| 7  | Petro | 1500   | IT         |

### Варіант 3 (MAX)

### Запит

```sql
DELETE FROM employees
-- які не входять у список максимальних
WHERE id NOT IN (
    -- вибираємо найбільший id у кожній групі
    SELECT MAX(id)
    FROM employees
    -- групуємо
    GROUP BY name, salary, department
);
```

### Результат

| id | name  | salary | department |
|----|-------|--------|------------|
| 3  | Ivan  | 1000   | IT         |
| 4  | Ivan  | 1000   | HR         |
| 6  | Olena | 2000   | Finance    |
| 7  | Petro | 1500   | IT         |

## Задача №2

Отримати всіх співробітників, незалежно від того, чи мають вони відповідності відділу департаменті.

Вивести середню зарплату по відділах.

### EMPLOYEES

| LAST_NAME | DEPARTMENT_ID | SALARY  |
|-----------|---------------|---------|
| Getz      | 10            | 3000    |
| Davis     | 20            | 1500    |
| King      | 20            | 2200    |
| Davis     | 30            | 5000    |
| Kochhar   |               | 5000    |

### DEPARTMENTS

| DEPARTMENT_ID | DEPARTMENT_NAME |
|---------------|-----------------|
| 10            | Sales           |
| 20            | Marketing       |
| 30            | Accounts        |
| 40            | Administration  |

## Підготовка

### Створення таблиць

```sql
CREATE TABLE departments (
    department_id INT PRIMARY KEY,
    department_name TEXT NOT NULL
);
```

```sql
CREATE TABLE employees (
    last_name TEXT NOT NULL,
    department_id INT,
    salary INT NOT NULL,
    FOREIGN KEY (department_id)
    REFERENCES departments(department_id)
);
```

### Наповнення таблиць

```sql
INSERT INTO departments (department_id, department_name) VALUES
(10, 'Sales'),
(20, 'Marketing'),
(30, 'Accounts'),
(40, 'Administration');
```

```sql
INSERT INTO employees (last_name, department_id, salary) VALUES
('Getz', 10, 3000),
('Davis', 20, 1500),
('King', 20, 2200),
('Davis', 30, 5000),
('Kochhar', NULL, 5000);
```

## Отримати всіх співробітників
(незалежно від того, чи мають вони відповідності відділу департаменті)

### Запит

```sql
SELECT
    e.last_name,
    e.department_id,
    d.department_name,
    e.salary
FROM employees e
 -- приєднуємо таблицю відділів
LEFT JOIN departments d
-- по department_id
ON e.department_id = d.department_id;  
```

### Результат

| last_name  | department_id  | department_name | salary |
|------------|----------------|-----------------|--------|
| Getz       | 10             | Sales           | 3000   |
| Davis      | 20             | Marketing       | 1500   |
| King       | 20             | Marketing       | 2200   |
| Davis      | 30             | Accounts        | 5000   |
| Kochhar    | NULL           | NULL            | 5000   |

## Вивести середню зарплату по відділах

```sql
SELECT
    d.department_id,
    d.department_name,
    -- середня зарплата по відділу
    AVG(e.salary) AS avg_salary
-- беремо всі відділи
FROM departments d
    -- приєднуєм людей
    LEFT JOIN employees e
    -- по department_id
    ON d.department_id = e.department_id
GROUP BY
    -- групуємо по department_id + department_name
    d.department_id,                
    d.department_name
-- якшо треба сортуємо
ORDER BY avg_salary DESC;      
```

### Результат

| department_id | department_name | avg_salary |
|---------------|-----------------|------------|
| 40            | Administration  | NULL       |
| 30            | Accounts        | 5000       |
| 10            | Sales           | 3000       |
| 20            | Marketing       | 1850       |


## Задача №3

Потрібно вибрати ідентифікатор відділу, мінімальний розмір заробітної плати, а також максимальну зарплату, виплачену в цьому відділі, з урахуванням, що мінімальна заробітна плата становить менше 5000, і максимальна зарплата більша, ніж 15000.

### Опис таблиці EMPLOYEES

| COLUMN_NAME | DATA_TYPE    |             |
|-------------|--------------|-------------|
| EMP_ID      | NUMBER(4)    | NOT NULL    |
| LAST_NAME   | VARCHAR2(30) | NOT NULL    |
| FIRST_NAME  | VARCHAR2(30) |             |
| DEPT_ID     | NUMBER(2)    |             |
| JOB_CAT     | VARCHAR2(30) |             |
| SALARY      | NUMBER       |             |

## Підготовка

### Створення таблиці

```sql
CREATE TABLE employees (
    emp_id INT PRIMARY KEY,
    last_name VARCHAR(30) NOT NULL,
    first_name VARCHAR(30),
    dept_id INT,
    job_cat VARCHAR(30),
    salary NUMERIC
);
```

### Наповнення таблиці

```sql
INSERT INTO employees (emp_id, last_name, first_name, dept_id, job_cat, salary) VALUES
(1, 'Smith', 'John', 10, 'Manager', 4000),
(2, 'Brown', 'Kate', 10, 'Clerk', 16000),
(3, 'White', 'Tom', 20, 'Analyst', 3000),
(4, 'Black', 'Anna', 20, 'Director', 14000),
(5, 'Green', 'Olga', 30, 'Clerk', 2000),
(6, 'Taylor', 'Max', 30, 'Manager', 18000),
(7, 'Wilson', 'Eva', 40, 'Clerk', 6000),
(8, 'Moore', 'Liam', 40, 'Manager', 12000);
```

Для кожного dept_id реба порахувати MIN і MAX salary і вивести результат у вигляді dept_id min_salary max_salary. Залишити в результаті тільки ті відділи де min_salary < 5000 і max_salary > 15000.

### Запит

```sql
SELECT
    dept_id,
    MIN(salary) AS min_salary,
    MAX(salary) AS max_salary
FROM employees
-- групуємо по відділах
GROUP BY dept_id
-- min_salary < 5000 і max_salary > 15000
HAVING MIN(salary) < 5000
   AND MAX(salary) > 15000;
```

### Результат

| dept_id | min_salary | max_salary |
|---------|------------|------------|
| 30      | 2000       | 18000      |
| 10      | 4000       | 16000      |


## Задача №4

В таблиці EMPLOYEES, EMPLOYEE_ID є первинним ключем.

MGR_ID це ідентифікатор менеджерів і відноситься до EMPLOYEE_ID.

Dept_id є зовнішнім ключем до DEPARTMENT_ID колонки таблиці DEPARTMENTS.

В таблиці DEPARTMENTS department_id є первинним ключем.
 
## EMPLOYEES

| EMPLOYEE_ID | EMP_NAME | DEPT_ID | MGR_ID | JOB_ID    | SALARY |
|-------------|----------|---------|--------|-----------|--------|
| 101         | Smith    | 20      | 120    | SA_REP    | 4000   |
| 102         | Martin   | 10      | 105    | CLERK     | 2500   |
| 103         | Chris    | 20      | 120    | IT_ADMIN  | 4200   |
| 104         | John     | 30      | 108    | HR_CLERK  | 2500   |
| 105         | Diana    | 30      | 108    | IT_ADMIN  | 5000   |
| 106         | Smith    | 40      | 110    | AD_ASST   | 3000   |
| 108         | Jennifer | 30      | 110    | HR_DIR    | 6500   |
| 110         | Bob      | 40      |        | EX_DIR    | 8000   |
| 120         | Ravi     | 20      | 110    | SA_DIR    | 6500   |

## DEPARTMENTS

| DEPARTMENT_ID | DEPARTMENT_NAME  |
|---------------|------------------|
| 10            | Admin            |
| 20            | Education        |
| 30            | IT               |
| 40            | Human Resources  |

## Підготовка

### Створення таблиць

```sql
CREATE TABLE departments (
    department_id INT PRIMARY KEY,
    department_name VARCHAR(100) NOT NULL
);
```

```sql
CREATE TABLE employees (
    employee_id INT PRIMARY KEY,
    emp_name VARCHAR(100) NOT NULL,
    dept_id INT,
    mgr_id INT,
    job_id VARCHAR(50),
    salary INT,
    FOREIGN KEY (dept_id)
    REFERENCES departments(department_id),
    FOREIGN KEY (mgr_id)
    REFERENCES employees(employee_id)
);
```

### Наповнення таблиць

```sql
INSERT INTO departments (department_id, department_name) VALUES
(10, 'Admin'),
(20, 'Education'),
(30, 'IT'),
(40, 'Human Resources');
```

```sql
INSERT INTO employees (employee_id, emp_name, dept_id, mgr_id, job_id, salary) VALUES
(110, 'Bob', 40, NULL, 'EX_DIR', 8000),
(120, 'Ravi', 20, 110, 'SA_DIR', 6500),
(108, 'Jennifer', 30, 110, 'HR_DIR', 6500),
(105, 'Diana', 30, 108, 'IT_ADMIN', 5000),
(101, 'Smith', 20, 120, 'SA_REP', 4000),
(102, 'Martin', 10, 105, 'CLERK', 2500),
(103, 'Chris', 20, 120, 'IT_ADMIN', 4200),
(104, 'John', 30, 108, 'HR_CLERK', 2500),
(106, 'Smith', 40, 110, 'AD_ASST', 3000);
```

## Питання 1

Що станеться, якщо виконати
```sql
DELETE
FROM departments
WHERE department id = 40;
```
та чому?

## Відповідь

#### Що станеться?
Виникне помилка [23503] ERROR: update or delete on table "departments" violates foreign key constraint "employees_dept_id_fkey" on table "employees" Detail: Key (department_id)=(40) is still referenced from table "employees"

#### Чому?
Бо на нього існує посилання через foreign key + не було вказано ON DELETE CASCADE

## Питання 2

В таблиці EMPLOYEES, EMPLOYEE_ID є первинним ключем.

MGR_ID це ідентифікатор менеджерів і відноситься до EMPLOYEE_ID.

Колонка JOB_ID – NOT NULL.

## EMPLOYEES

| EMPLOYEE_ID | EMP_NAME | DEPT_ID | MGR_ID | JOB_ID    | SALARY |
|-------------|----------|---------|--------|-----------|--------|
| 101         | Smith    | 20      | 120    | SA_REP    | 4000   |
| 102         | Martin   | 10      | 105    | CLERK     | 2500   |
| 103         | Chris    | 20      | 120    | IT_ADMIN  | 4200   |
| 104         | John     | 30      | 108    | HR_CLERK  | 2500   |
| 105         | Diana    | 30      | 108    | IT_ADMIN  | 5000   |
| 106         | Smith    | 40      | 110    | AD_ASST   | 3000   |
| 108         | Jennifer | 30      | 110    | HR_DIR    | 6500   |
| 110         | Bob      | 40      |        | EX_DIR    | 8000   |
| 120         | Ravi     | 20      | 110    | SA_DIR    | 6500   |


Що станеться, якщо виконати 

```sql
DELETE employee_id, salary, job_id
FROM employees
WHERE dept_id = 90;
```

та чому?

## Відповідь
#### Що станеться? 
Якшо саме такий запит виконувати то він не виконається.

#### Чому? 
Через синтксичну помилку.

Якшо мався на увазі такий запит

```sql
DELETE FROM employees
WHERE dept_id = 90;
```

#### Що станеться?
Видаляться рярки з таблиці employees у яких dept_id = 90 (в прикладі таких нема)
