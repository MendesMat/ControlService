# Authentication

Signing in, temporary passwords, lockout and sessions. The account data itself (login, e-mail, status) belongs to [users](users.md).

- **Screens:** sign-in and create new password (the mandatory change). They have their own routes and do not open tabs.
- **Decisions:** in the [decisions document](../../decisoes-de-arquitetura.md): 16 (system records), 18 (Identity), 19 (access token), 20 (sessions), 21 (temporary password and the Admin demo account), 22 (lockout and rate limiting).
- **Read with:** [conventions](../conventions.md), [API conventions](../../api/conventions.md).

**Not built yet.** These rules are the decided behavior. Until the users endpoints are delivered (issue #11), the code still follows the previous plan: only the Admin has a mandatory password change, its initial password comes from the configuration, and registered users have no password. Decision 21 lists what is left over.

Access by e-mail (activation links, "Esqueci minha senha") is postponed until the product cycle is closed; the rules that described it are retired below.

## Accounts and temporary passwords

| ID | Rule |
|---|---|
| AUTH-01 | Nobody creates their own account. A new user is always registered by someone with access to the Users screen, who sets a **temporary password** and passes it on outside the system. The temporary password stops being valid as soon as the person signs in (AUTH-14); from then on, nobody knows another person's password. |
| AUTH-02 | A new user is created **active** (`active`), with the temporary password set at registration (USR-35) and a mandatory password change (AUTH-14). |
| AUTH-03 | *Retired: activation links are postponed (decision 21). It set the link as single-use and valid for 72 hours.* |
| AUTH-04 | *Retired: activation links are postponed (decision 21). It described "Reenviar acesso"; AUTH-29 replaces it.* |
| AUTH-05 | *Retired: no e-mail is sent (decision 21). It required saving the user before sending the activation e-mail.* |
| AUTH-06 | *Retired: no e-mail is sent (decision 21). It described what happened when sending failed.* |
| AUTH-29 | Someone with the Manager level on the Users screen can **reset the password** of another user ("Redefinir senha"): they set a new temporary password, the mandatory change comes back (AUTH-14), every session of that person ends and an active lockout is cleared. This is the only way back for someone who forgot the password. It is never available for the Admin (AUTH-15). |

## Signing in

| ID | Rule |
|---|---|
| AUTH-07 | People sign in with **login and password**. The e-mail is not used to sign in: it is optional, and several people may share it. |
| AUTH-08 | After **5 consecutive failed attempts**, that login is locked for **15 minutes**. The sign-in screen never reveals whether the login or the password was wrong. The **5th** wrong password already answers as locked; while locked, even the right password is refused and those attempts are not counted; a successful sign-in resets the count. |
| AUTH-09 | A deactivated account cannot sign in, even with the correct password. |
| AUTH-26 | Too many sign-in or refresh requests from the same address are refused (429 `locked_out`, with `Retry-After`) with *"Muitas tentativas em pouco tempo. Aguarde alguns instantes e tente de novo."* This is not the lockout of AUTH-08, whose message states the 15 minutes. |

**Known limitation.** A login that does not exist is never locked, so five attempts show whether a login exists. This is Identity's standard behavior, and the per-address rate limit of AUTH-26 makes scanning logins slow.

## Forgot password

There is no "Esqueci minha senha" for now. Someone who forgot the password asks for a reset (AUTH-29).

| ID | Rule |
|---|---|
| AUTH-10 | *Retired: reset links are postponed (decision 21). It sent a reset link, valid for 2 hours, to the e-mail of the login provided.* |
| AUTH-11 | *Retired: reset links are postponed (decision 21). It kept the response identical whether or not the login existed.* |
| AUTH-12 | *Retired: reset links are postponed (decision 21). It cleared an active lockout when a password was created through the link; AUTH-29 does that now.* |

## Mandatory password change

| ID | Rule |
|---|---|
| AUTH-14 | An account with a temporary password, set at registration (AUTH-02) or by a reset (AUTH-29), must replace it right after signing in. The system shows **"Crie uma senha nova"**: *"Por segurança, a senha temporária precisa ser trocada antes de continuar."* Until the password is changed, no other screen or operation is available. |
| AUTH-24 | Changing the password through the change-password operation is only for the account with a mandatory change. For any other account it is refused (403 `forbidden`) with *"Sua senha já foi criada. Para trocá-la, fale com o responsável pelo sistema."* |

## The Admin demo account

| ID | Rule |
|---|---|
| AUTH-13 | While the project is a portfolio, the Admin is the **demo account**: the login `admin` and the password `admin123` are fixed and shown on the sign-in screen, so any visitor can explore the whole system (decision 21). |
| AUTH-15 | The Admin never has a mandatory password change and cannot change its password: AUTH-24 refuses it like any account without a mandatory change, and its password cannot be reset (AUTH-29). |

## Passwords

| ID | Rule |
|---|---|
| AUTH-16 | A password has at least **8 characters**, with no requirement for symbols or digits: long passwords are safer and easier to remember than short complicated ones. The person creating their own password types it twice to confirm. Passwords are never trimmed: spaces are part of the password. The same minimum applies to a temporary password. |
| AUTH-25 | The new password must be different from the temporary one: *"A nova senha precisa ser diferente da senha temporária."* (field `password`) |

## Sessions

| ID | Rule |
|---|---|
| AUTH-17 | A session ends after **8 hours without any action**. In the back-end: a 15-minute access token kept only in the page's memory, and a refresh token in an `HttpOnly` cookie, valid for 8 hours from the last refresh and rotated on each use (decisions 19 and 20). The front-end refreshes only when it needs to make a request, never on a timer. |
| AUTH-18 | The server checks on **every request** that the account is still active. A deactivated person is rejected on their next request, even with a valid access token. Refresh is anonymous (it carries only the cookie), so it makes the same check itself: a non-active account is refused with `account_inactive` and its session is removed. |
| AUTH-27 | The server's `message` for 401 `session_expired`, `password_change_required` and `account_inactive` (during a session) is, respectively, the "Session ended" message, the AUTH-14 text and the "Deactivated account" message. A missing, invalid or expired access token is answered as `session_expired`. |

When a session ends, the sign-in screen appears **over** the system without closing the tabs. If the same person signs in again, everything is where it was, including unsaved forms.

## Credentials

| ID | Rule |
|---|---|
| AUTH-19 | Passwords and the lockout counter are **not part of the user record**. They are stored separately, only hashed, and never returned by any operation. |
| AUTH-20 | Passwords travel only in the request body and the refresh token only in its cookie: never in the URL, and neither is ever written to logs. |
| AUTH-21 | Deactivating a user ends every session of that person. |
| AUTH-22 | Every duration and limit in this document is an initial value, adjustable in the server configuration. |
| AUTH-23 | *Retired: activation links are postponed (decision 21). It refused activating a user who was not pending, as an invalid link.* |
| AUTH-28 | Messages that state a duration or a limit show the configured value (AUTH-22): with the defaults, the texts below are exact; with 30 minutes of lockout, the message says "Aguarde 30 minutos". |

## Messages

Verbatim, in Portuguese.

| Situation | Message |
|---|---|
| User created | Cadastro de *{nome}* criado. Passe a senha temporária para a pessoa: ela vai criar a própria senha ao entrar. |
| Temporary password left blank | Informe a senha temporária. |
| Password reset (AUTH-29) | Senha temporária de *{nome}* definida. A pessoa vai criar a própria senha ao entrar. |
| Wrong login or password | Login ou senha incorretos. |
| Locked out | Muitas tentativas sem sucesso. Aguarde 15 minutos e tente de novo. |
| Too many requests (rate limit) | Muitas tentativas em pouco tempo. Aguarde alguns instantes e tente de novo. |
| Deactivated account (correct password) | Este acesso está desativado. Fale com o responsável pelo sistema. |
| Password too short | A senha precisa ter pelo menos 8 caracteres. |
| Passwords differ | As duas senhas não são iguais. Digite de novo. |
| New password equal to the temporary one | A nova senha precisa ser diferente da senha temporária. |
| Change password without a mandatory change | Sua senha já foi criada. Para trocá-la, fale com o responsável pelo sistema. |
| Temporary password must be changed (`password_change_required`) | Por segurança, a senha temporária precisa ser trocada antes de continuar. |
| Temporary password replaced | Senha criada. Tudo pronto para começar. |
| Session ended | Sua sessão terminou. Entre de novo para continuar. Suas abas continuam abertas. |
| Signed out with the Sair button | Você saiu do sistema. |
| Sign out with unsaved changes | **Sair com alterações não salvas?** Algumas abas têm alterações que ainda não foram salvas. Se você sair agora, elas serão perdidas. |

## Operations

| Front-end method | Route | Who | Notes |
|---|---|---|---|
| `signIn(login, password)` | `POST /api/v1/auth/sign-in` | Anyone | 200 `{ accessToken, expiresIn, mustChangePassword }`; sets the refresh cookie. `expiresIn` is in **seconds** (900), not a timestamp, so a wrong clock on the person's computer does not break the refresh decision |
| `hasSession()` | `POST /api/v1/auth/refresh` | Anyone | Uses the refresh cookie; called when the page opens. Same 200 body as sign-in and a rotated cookie; without a valid session, 401 `session_expired` |
| `signOut()` | `POST /api/v1/auth/sign-out` | Signed in | 204. Ends the session named by the `sid` claim of the access token (the cookie is scoped to the refresh route and never arrives here) and expires the cookie; the person's other sessions stay open |
| `changePassword(password, confirmation)` | `POST /api/v1/auth/change-password` | Signed in | Only for the account with a mandatory change (AUTH-24). Same 200 body as sign-in; ends every session of the person and starts a new one |
| `me()` | `GET /api/v1/me` | Signed in | `{ id, displayName, login, levels }`, where `levels` is `[{ "screen": "<key>", "level": "<wire value>" }]` with **every** screen of the catalog, in catalog order, including `negado` (same shape as a profile's `levels`) |

Resetting someone's password (AUTH-29) is an operation of the [users](users.md#operations) feature.

Sign-in and refresh are rate limited (decision 22, AUTH-26). Until the temporary password is replaced, every operation except `me`, sign-out and change-password answers 401 `password_change_required` (AUTH-14).

## Errors

| Status | `code` | When |
|---|---|---|
| 401 | `invalid_credentials` | Wrong login or password |
| 403 | `account_inactive` | **At sign-in**: correct password, deactivated account |
| 401 | `account_inactive` | **During a session**, including at refresh: the account was deactivated (AUTH-18) |
| 401 | `password_change_required` | The temporary password was not changed yet (AUTH-14) |
| 401 | `session_expired` | The session ended |
| 403 | `forbidden` | Change password on an account without a mandatory change (AUTH-24) |
| 429 | `locked_out` | Lockout (AUTH-08) or rate limit (AUTH-26), with `Retry-After` and `details.retryAfterSeconds` (the seconds left, rounded up) |
| 400 | `validation_failed` | Password rules; fields `password` and `passwordConfirmation` |

The error format and what the front-end does with each code are in the [API conventions](../../api/conventions.md#errors).
