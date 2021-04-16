<script lang="ts">
  import { onMount } from 'svelte';
  import * as autosize from 'autosize';

  let messageBox: HTMLTextAreaElement;

  onMount(() => {
    autosize.default(messageBox);
  });
</script>

<div class="absolute px-4 bottom-4 w-full">
  <div class="flex items-end dark:bg-gray-300 rounded pr-4">
    <div class="inline-block flex-1 align-bottom transform transition-all">
      <textarea
        bind:this={messageBox}
        placeholder="Message #General"
        class="block w-full h-12 resize-none p-4 outline-none rounded-tl rounded-bl dark:text-white dark:placeholder-white-dimmest" />
    </div>
    <div class="inline-block flex-none">
      Tt
    </div>
  </div>
</div>

<style>
  /* Force overwrite user agent stylesheet */
  textarea {
    background-color: rgba(0, 0, 0, 0) !important;
  }
</style>