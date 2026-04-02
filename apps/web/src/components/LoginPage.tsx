import { useState } from "react";
import { loginAuth, registerAuth } from "../api/chatApi";
import "./LoginPage.css";

type Props = {
  onAuth: (userId: string, username: string, displayName: string) => void;
};

export function LoginPage({ onAuth }: Props) {
  const [tab, setTab] = useState<"login" | "register">("login");
  const [username, setUsername] = useState("");
  const [displayName, setDisplayName] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);

  const submit = async () => {
    if (!username.trim() || !password.trim()) {
      setError("Username and password are required.");
      return;
    }
    setError("");
    setLoading(true);
    try {
      const res =
        tab === "login"
          ? await loginAuth({ username: username.trim(), password })
          : await registerAuth({
              username: username.trim(),
              password,
              displayName: displayName.trim() || username.trim(),
            });
      onAuth(res.userId, res.username, res.displayName);
    } catch (e) {
      setError(String(e).replace(/^Error:\s*/, ""));
    } finally {
      setLoading(false);
    }
  };

  const onKey = (e: React.KeyboardEvent) => {
    if (e.key === "Enter") void submit();
  };

  const switchTab = (t: "login" | "register") => {
    setTab(t);
    setError("");
  };

  return (
    <div className="lp-root">
      <div className="lp-card">
          <h2 className="lp-heading">
            {tab === "login" ? "Sign in" : "Create account"}
          </h2>

          <div className="lp-tabs">
            <button
              className={tab === "login" ? "active" : ""}
              onClick={() => switchTab("login")}
            >
              Sign in
            </button>
            <button
              className={tab === "register" ? "active" : ""}
              onClick={() => switchTab("register")}
            >
              Register
            </button>
          </div>

          <div className="lp-fields">
            <label>Username</label>
            <input
              value={username}
              onChange={(e) => setUsername(e.target.value)}
              placeholder="your_username"
              autoComplete="username"
              onKeyDown={onKey}
            />

            {tab === "register" && (
              <>
                <label>
                  Display name{" "}
                  <span className="lp-optional">(optional)</span>
                </label>
                <input
                  value={displayName}
                  onChange={(e) => setDisplayName(e.target.value)}
                  placeholder="Your full name"
                  onKeyDown={onKey}
                />
              </>
            )}

            <label>Password</label>
            <input
              type="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              placeholder="••••••••"
              autoComplete={
                tab === "login" ? "current-password" : "new-password"
              }
              onKeyDown={onKey}
            />
          </div>

          {error && <div className="lp-error">{error}</div>}

          <button
            className="lp-submit"
            disabled={loading}
            onClick={() => void submit()}
          >
            {loading
              ? "Please wait…"
              : tab === "login"
              ? "Sign in"
              : "Create account"}
          </button>

          <p className="lp-switch">
            {tab === "login" ? (
              <>
                Don't have an account?{" "}
                <button onClick={() => switchTab("register")}>Register</button>
              </>
            ) : (
              <>
                Already have an account?{" "}
                <button onClick={() => switchTab("login")}>Sign in</button>
              </>
            )}
          </p>
        </div>
    </div>
  );
}
