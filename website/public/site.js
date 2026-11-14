for (const button of document.querySelectorAll(".copy")) {
  if (!navigator.clipboard?.writeText) continue;
  button.hidden = false;
  const code = button.closest(".code-block").querySelector("code");
  let reset;
  button.addEventListener("click", async () => {
    clearTimeout(reset);
    try {
      await navigator.clipboard.writeText(code.textContent);
      button.textContent = "Copied";
    } catch {
      button.textContent = "Copy failed";
    }
    reset = setTimeout(() => { button.textContent = "Copy"; }, 1800);
  });
}
