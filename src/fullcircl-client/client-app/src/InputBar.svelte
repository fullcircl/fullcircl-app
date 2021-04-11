<script>
  let message = '';
  let taHeight = 0;
</script>

<div class="absolute px-4 bottom-4 w-full">
  <div class="flex items-end dark:bg-gray-300 rounded">
    <div class="inline-block flex-1 align-bottom transform transition-all">
      <textarea bind:value={message} bind:clientHeight={taHeight} placeholder="Message #General" class="block w-full resize-none outline-none rounded-tl rounded-bl dark:text-white-50 dark:placeholder-white-900" />
      <div bind:clientHeight={taHeight} class="block w-full dark:text-white-50 dark:placeholder-white-900">
        { message }
      </div>
    </div>
    <div class="inline-block flex-none">
      TT
    </div>
  </div>
</div>

<style>
  /* Force overwrite user agent stylesheet */
  textarea {
    background-color: rgba(0, 0, 0, 0) !important;
  }
</style>