<script>
  let rowCount = 1;

  function handleMessageInput(event) {
    rowCount = (event.target.value.match(/\n/g) || []).length + 1;
  }
</script>

<div class="absolute px-4 bottom-4 w-full">
  <div class="flex items-end dark:bg-gray-300 rounded">
    <div class="inline-block flex-1 align-bottom transform transition-all">
      <textarea
        on:input={handleMessageInput}
        rows={rowCount}
        placeholder="Message #General"
        class="block w-full resize-none outline-none rounded-tl rounded-bl dark:text-white-50 dark:placeholder-white-900" />
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