<script lang="ts">
	import InputBar from "./InputBar.svelte";
import Message from "./Message.svelte";
	import Navbar from "./Navbar.svelte";
	import Tailwindcss from "./Tailwindcss.svelte";
</script>

<Tailwindcss />
<main class="font-mono dark:bg-gray-700 dark:text-white">
	<Navbar />
	<Message />
	<InputBar />
</main>

<style>
</style>
