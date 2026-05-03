(function () {
  const status = document.getElementById("status");
  const messageBox = document.getElementById("message");
  const button = document.getElementById("siwe-btn");

  function setStatus(text) {
    status.textContent = text;
  }

  function toIsoUtc(date) {
    return new Date(date).toISOString();
  }

  function randomNonce32() {
    const values = new Uint8Array(16);
    crypto.getRandomValues(values);
    return Array.from(values, b => b.toString(16).padStart(2, "0")).join("");
  }

  function buildSiweMessage(domain, address, nonce) {
    const issuedAt = toIsoUtc(Date.now());
    return `${domain} wants you to sign in with your Ethereum account:
${address}

Sign in to SovereignID demo

URI: ${globalThis.location.origin}
Version: 1
Chain ID: 11155111
Nonce: ${nonce}
Issued At: ${issuedAt}`;
  }

  async function signIn() {
    if (!globalThis.ethereum) {
      setStatus("No se detecto MetaMask. Instala la extension para continuar.");
      return;
    }

    try {
      setStatus("Solicitando nonce al backend...");
      const nonceResponse = await fetch("/auth/nonce");
      if (!nonceResponse.ok) {
        throw new Error(`GET /auth/nonce fallo con estado ${nonceResponse.status}`);
      }

      const nonceBody = await nonceResponse.json();
      const nonce = nonceBody.nonce || randomNonce32();

      setStatus("Solicitando cuenta de MetaMask...");
      const accounts = await globalThis.ethereum.request({ method: "eth_requestAccounts" });
      const account = accounts && accounts.length > 0 ? accounts[0] : null;
      if (!account) {
        throw new Error("No se recibio una cuenta desde MetaMask.");
      }

      const message = buildSiweMessage(globalThis.location.host, account, nonce);
      messageBox.textContent = message;

      setStatus("Solicitando firma de SIWE...");
      const signature = await globalThis.ethereum.request({
        method: "personal_sign",
        params: [message, account]
      });

      setStatus("Enviando firma para verificacion...");
      const verifyResponse = await fetch("/auth/verify", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ message, signature })
      });

      const verifyBody = await verifyResponse.json();
      if (!verifyResponse.ok) {
        const code = verifyBody?.error || "unknown_error";
        const detail = verifyBody?.detail || "Sin detalle";
        throw new Error(`${code}: ${detail}`);
      }

      setStatus(
        `Sesion creada.\nAddress: ${verifyBody.address}\nExpires: ${verifyBody.expiresAt}\nJWT: ${verifyBody.jwt}`
      );
    } catch (error) {
      const message = error instanceof Error ? error.message : "Error desconocido";
      setStatus(`Error: ${message}`);
    }
  }

  button.addEventListener("click", signIn);
})();
