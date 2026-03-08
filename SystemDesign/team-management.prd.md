# Product Requirements Document (PRD)
## Module: Team Management

## 1. Overview
The Team Management module allows the system administrator to create and manage teams within the Complaint Management System. Teams help organize agents and supervisors responsible for handling customer complaints.

This module enables administrators to:
- Create teams
- Assign team members
- Update team details
- Delete teams

The module is designed to support a small team structure (maximum 10 users).

---

# 2. Objectives

The objective of the Team Management module is to:

- Organize system users into teams
- Assign agents to specific teams
- Allow supervisors to manage team members
- Simplify complaint assignment and responsibility management

---

# 3. User Roles

Only the following role can manage teams:

| Role | Permission |
|-----|------------|
| Admin | Full access to create, edit, delete teams and assign members |

Other roles:
- Supervisor
- Agent
- Client

These roles cannot manage teams.

---

# 4. Functional Requirements

## 4.1 Create Team

Admin should be able to create a new team.

### Input Fields

| Field | Type | Required | Description |
|------|------|---------|-------------|
| Team Name | Text | Yes | Name of the team |
| Lead User ID | Dropdown | Optional | Supervisor assigned as team leader |

### Rules

- Team name must be unique
- Lead user must be a valid system user
- Maximum 10 users in system

### Action

Click **Create** button to create the team.

---

## 4.2 Assign Team Members

Admin should be able to assign users to a team.

### Actions

- Select a team
- Add users as team members

### Rules

- Only **Agents or Supervisors** can be added
- A user can belong to **one team only**
- Maximum team capacity: **5 members**

---

## 4.3 Edit Team

Admin should be able to edit team details.

### Editable Fields

| Field | Description |
|------|-------------|
| Team Name | Update team name |
| Lead User | Change team leader |

### Action

Click **Edit** → Update details → Save.

---

## 4.4 Update Team Members

Admin should be able to:

- Add new members
- Remove existing members
- Change team leader

Changes should be reflected immediately in the system.

---

## 4.5 Delete Team

Admin should be able to delete a team.

### Rules

- System should confirm before deletion
- If team has members, system should require removing members first OR automatically remove them

### Confirmation Message


---

# 5. UI Components

Based on the current UI design.

### Team Creation Section

Fields:
- Team Name
- Lead User ID (optional)

Buttons:
- Create

---

### Team List Table

Columns:

| Column | Description |
|------|-------------|
| Team | Team name |
| Lead | Team leader |
| Members | Number of members |
| Status | Active / Inactive |
| Actions | Edit / Delete |

---

# 6. Validation Rules

| Rule | Description |
|-----|-------------|
| Unique Team Name | Duplicate team names are not allowed |
| Valid Lead User | Lead user must exist in system |
| Team Size Limit | Maximum 5 members per team |

---

# 7. Non-Functional Requirements

| Requirement | Description |
|-------------|-------------|
| Performance | Team creation should take less than 2 seconds |
| Security | Only Admin can manage teams |
| Usability | Interface should be simple and easy to use |

---

# 8. Future Enhancements (Optional)

These features may be added later:

- Team performance dashboard
- Automatic complaint assignment
- Team workload monitoring
- SLA tracking per team
- Team-based reporting

---

# 9. Acceptance Criteria

The module is considered complete when:

- Admin can create teams
- Admin can assign members
- Admin can edit team details
- Admin can delete teams
- Team list displays correctly in UI