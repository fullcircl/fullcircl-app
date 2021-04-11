<script lang="ts">
  import InputBar from "./InputBar.svelte";
  import MessageArea from "./MessageArea.svelte";
  import Navbar from "./Navbar.svelte";
  import Tailwindcss from "./Tailwindcss.svelte";
</script>

<Tailwindcss />
<main class="font-mono dark:bg-gray dark:text-white">
  <div class="flex flex-col h-full">
    <Navbar />
    <MessageArea />
  </div>
  
  <InputBar />
</main>

<style>
</style>
