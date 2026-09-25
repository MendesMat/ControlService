# Authentication

Signing in, account activation, password reset, lockout and sessions. The account data itself (login, e-mail, status) belongs to [users](users.md).

- **Screens:** sign-in, activate account, forgot password, create new password, first access of the Admin. They have their own routes and do not open tabs.
- **Decisions:** ADR-0019 (Identity, JWT, activation links), ADR-0022 (Admin bootstrap), ADR-0023 (rate limiting), ADR-0030 and ADR-0031 (e-mails), ADR-0032 (session lifetime).
- **Read with:** [conventions](../conventions.md), [API conventions](../../api/conventions.md).

## Accounts and activation

| ID | Rule |
|---|---|
| AUTH-01 | Nobody creates their own account. A new user is always registered by someone with access to the Users screen, and nobody, at any moment, sees or types another person's password. |
| AUTH-02 | A new user is created as **pending** (`pending`), and the system sends an **activation link** to the registered e-mail. Opening the link, the person creates their own password and the account becomes **active** (`active`). |
| AUTH-03 | The activation link is single-use and valid for **72 hours**. |
| AUTH-04 | While the user is pending and has an e-mail, their record shows **Reenviar acesso** (resend access) in the "Acesso ao sistema" section. It sends a new link to the e-mail saved at that moment and invalidates the previous link. |
| AUTH-05 | The user is **always saved before** the e-mail is sent: the link must point to an existing record, and the login and display name must be reserved from the start. The e-mail is sent right after, in the same operation, and the response says whether it left (`emailSent`, ADR-0031). |
| AUTH-06 | If sending fails, the user stays registered as pending and the screen says so, so someone can use "Reenviar acesso". Nobody ever needs to delete and register the user again: if the e-mail was wrong, fix it, save and resend. |

If the record has unsaved changes, "Reenviar acesso" asks to save first: *"Este cadastro tem alterações que ainda não foram salvas. Salve primeiro, para o link ir para o e-mail correto."*

## Signing in

| ID | Rule |
|---|---|
| AUTH-07 | People sign in with **login and password**. The e-mail is not used to sign in, because several people may share it. |
| AUTH-08 | After **5 consecutive failed attempts**, that login is locked for **15 minutes**. The sign-in screen never reveals whether the login or the password was wrong. |
| AUTH-09 | A deactivated account cannot sign in, even with the correct password. |

## Forgot password

| ID | Rule |
|---|---|
| AUTH-10 | The person provides their **login**, not the e-mail (see AUTH-07). If the login exists and is active, the system sends a single-use link valid for **2 hours** to the registered e-mail. |
| AUTH-11 | The response is always the same, in content and in duration, whether or not the login exists, so it never reveals which logins exist. That is why reset e-mails are sent in the background (ADR-0031). |
| AUTH-12 | Creating a new password through the link also clears an active lockout. |

## First access of the Admin

| ID | Rule |
|---|---|
| AUTH-13 | The Admin is the only account without an activation link, because nobody can register it. It signs in with an initial password that comes from the server configuration, never from the code (ADR-0022). |
| AUTH-14 | Right after that first sign-in, the system shows **"Crie uma senha nova"**: *"Por segurança, a senha inicial precisa ser trocada no primeiro acesso."* Until the password is changed, no other screen or operation is available. |
| AUTH-15 | The Admin is a system record that "cannot be changed", but **can change its own password** and use "Esqueci minha senha": credentials are not part of the user record (AUTH-19). |

## Passwords

| ID | Rule |
|---|---|
| AUTH-16 | A password has at least **8 characters**, with no requirement for symbols or digits: long passwords are safer and easier to remember than short complicated ones. The person types it twice to confirm. |

## Sessions

| ID | Rule |
|---|---|
| AUTH-17 | A session ends after **8 hours without any action**. In the back-end: a 15-minute access token kept only in the page's memory, and a refresh token in an `HttpOnly` cookie, valid for 8 hours from the last refresh and rotated on each use (ADR-0019, ADR-0032). The front-end refreshes only when it needs to make a request, never on a timer. |
| AUTH-18 | The server checks on **every request** that the account is still active. A deactivated person is rejected on their next request, even with a valid access token. |

When a session ends, the sign-in screen appears **over** the system without closing the tabs. If the same person signs in again, everything is where it was, including unsaved forms.

## Credentials and links

| ID | Rule |
|---|---|
| AUTH-19 | Passwords, activation and reset links, and the lockout counter are **not part of the user record**. They are stored separately, only hashed, and never returned by any operation. |
| AUTH-20 | Links carry only a random token. Tokens travel in the request body, never in the URL, and are never written to logs (ADR-0030). |
| AUTH-21 | Deactivating a user invalidates their pending links and revokes their refresh tokens. |
| AUTH-22 | Every duration and limit in this document is an initial value, adjustable in the server configuration. |

## Messages

Verbatim, in Portuguese.

| Situation | Message |
|---|---|
| User created | Cadastro de *{nome}* criado. Enviamos o link de ativação para *{e-mail}*. |
| Activation e-mail failed | Cadastro de *{nome}* criado, mas não conseguimos enviar o e-mail de ativação. Use “Reenviar acesso” para tentar de novo. |
| Access resent | Enviamos um novo link de ativação para *{e-mail}*. |
| Wrong login or password | Login ou senha incorretos. |
| Locked out | Muitas tentativas sem sucesso. Aguarde 15 minutos e tente de novo. |
| Deactivated account (correct password) | Este acesso está desativado. Fale com o responsável pelo sistema. |
| Activation link expired or used | Este link não vale mais. Peça a quem cadastrou você para reenviar o acesso. |
| Reset link expired or used | Este link não vale mais. Peça um novo em “Esqueci minha senha”. |
| Password reset requested | Se esse login existir, enviamos um link para o e-mail cadastrado. Confira sua caixa de entrada. |
| Password too short | A senha precisa ter pelo menos 8 caracteres. |
| Passwords differ | As duas senhas não são iguais. Digite de novo. |
| Admin initial password changed | Senha criada. Tudo pronto para começar. |
| Session ended | Sua sessão terminou. Entre de novo para continuar. Suas abas continuam abertas. |
| Signed out with the Sair button | Você saiu do sistema. |
| Sign out with unsaved changes | **Sair com alterações não salvas?** Algumas abas têm alterações que ainda não foram salvas. Se você sair agora, elas serão perdidas. |

## Operations

| Front-end method | Route | Who | Notes |
|---|---|---|---|
| `signIn(login, password)` | `POST /api/v1/auth/sign-in` | Anyone | Returns `{ mustChangePassword }` |
| `hasSession()` | `POST /api/v1/auth/refresh` | Anyone | Uses the refresh cookie; called when the page opens |
| `signOut()` | `POST /api/v1/auth/sign-out` | Signed in | Revokes the refresh token |
| `changePassword(password, confirmation)` | `POST /api/v1/auth/change-password` | Signed in | Only for the account with a mandatory change |
| `checkLink(token, purpose)` | `POST /api/v1/auth/links/check` | Anyone | `purpose`: `activation` or `reset`. Returns `{ valid, displayName, login }` |
| `activateAccount(token, password, confirmation)` | `POST /api/v1/auth/activate` | Anyone | |
| `requestPasswordReset(login)` | `POST /api/v1/auth/password-reset/request` | Anyone | Always the same response (AUTH-11) |
| `resetPassword(token, password, confirmation)` | `POST /api/v1/auth/password-reset` | Anyone | |
| `me()` | `GET /api/v1/me` | Signed in | `{ id, displayName, login, levels }`, with the effective level on each screen |

Sign-in, refresh and the e-mail-sending operations are rate limited (ADR-0023).

## Errors

| Status | `code` | When |
|---|---|---|
| 401 | `invalid_credentials` | Wrong login or password |
| 403 | `account_inactive` | **At sign-in**: correct password, deactivated account |
| 401 | `account_inactive` | **During a session**: the account was deactivated (AUTH-18) |
| 401 | `password_change_required` | The initial password was not changed yet (AUTH-14) |
| 401 | `session_expired` | The session ended |
| 410 | `link_invalid` | Link expired or already used |
| 429 | `locked_out` | Lockout (AUTH-08) or rate limit, with `Retry-After` |
| 400 | `validation_failed` | Password rules; fields `password` and `passwordConfirmation` |

The error format and what the front-end does with each code are in the [API conventions](../../api/conventions.md#errors).
