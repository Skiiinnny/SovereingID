# Issuer bounded context — glossary

## Agreed scope (Phase 3, first milestone)

- **Formato acordado (primer incremento):** emitir credenciales como documentos **W3C Verifiable Credentials Data Model 1.1** (JSON-LD) con prueba de firma **EIP-712** según decisión de arquitectura del repo.
- **Primer incremento:** un **solo** perfil de credencial académica en JSON-LD (`@type` + claims acordados): **`TituloGraduacion`** (ver [`src/shared/CONTEXT.md`](../shared/CONTEXT.md)); los demás perfiles se añaden después reutilizando el mismo tubo.
- **Out of scope for this milestone:** subida a IPFS, anclaje de hash en Ethereum, contratos de registro/revocación en Sepolia; esos se modelan como **puertos** con implementaciones falsas o en memoria donde haga falta.
- **Holder / VP:** el emisor entrega **VC** al titular; la **Verifiable Presentation** la construye el titular (u otro contexto). El verificador consume **VP**; el emisor no emite VP en este modelo.

## Glossary

| Term | Meaning |
|------|--------|
| **Issuer** | La organización que firma y emite el VC al titular (en el producto: instituto). |
| **VC** | Verifiable Credential — documento JSON-LD firmado que hace afirmaciones sobre un `credentialSubject`. |
| **VP** | Verifiable Presentation — sobre W3C que empaqueta uno o más VCs y la prueba del titular; es lo que recibe el **Verifier** (acordado: el incremento actual incluye verificación sobre VP mínima). |
| **Subject DID (en VC)** | Valor de `credentialSubject.id` en credenciales emitidas: ver convención canónica en [`src/shared/CONTEXT.md`](../shared/CONTEXT.md). |
| **Issuer DID** | Valor de `issuer` en VCs emitidos: misma convención `did:ethr:sepolia:…` en [`src/shared/CONTEXT.md`](../shared/CONTEXT.md). |
| **TituloGraduacion** | Perfil de VC académico único en alcance del primer incremento; `type` incluye `TituloGraduacionCredential`; allowlist de claims en [`src/shared/CONTEXT.md`](../shared/CONTEXT.md). |

Further terms will be added as they are agreed in `/grill-with-docs` or implementation.
