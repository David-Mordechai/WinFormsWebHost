<script setup lang="ts">
import { ref, onUnmounted, onMounted } from 'vue';
import CommunicationService from './CommunicationService'

const command1BtnColor = ref('');
const command2BtnColor = ref('');
const isSocketConnected = ref<boolean>(false);
let socket: CommunicationService;

onMounted(async () => {
  socket = new CommunicationService("http://localhost:5000/ws/");

  socket.ConnectionState = (connected: boolean) => {
    isSocketConnected.value = connected;
  }

  socket.ReceiveReport = (color: string, command: string) => {
    switch (command) {
      case 'command1':
        command1BtnColor.value = color;
        break;
      case 'command2':
        command2BtnColor.value = color;
        break;
      default:
        break;
    }
  }

  await socket.start();
});

onUnmounted(async () => {
  await socket.stop();
})

function sendCommand(command: string) {
  socket.sendCommand(command);
}

async function callApi() {
  try {
    const response = await fetch("http://localhost:5000/api/hello");
    alert(await response.text());

  } catch (error) {
    console.error('Error calling API:', error);
  }
}
</script>

<template>
  <div class="parent">
    <div class="status" v-if="isSocketConnected === true" style="background-color: green;">Connected to server</div>
    <div class="status" v-if="isSocketConnected === false" style="background-color: red;">Disconnected from server</div>
    <button class="cmd1" @click="sendCommand('command1')" :style="{ backgroundColor: command1BtnColor }"
      v-bind:disabled="!isSocketConnected">Command1</button>
    <button class="cmd2" @click="sendCommand('command2')" :style="{ backgroundColor: command2BtnColor }"
      v-bind:disabled="!isSocketConnected">Command2</button>
    <button class="cmd3" @click="callApi">Call API</button>
  </div>
</template>

<style scoped>
.parent {
  display: grid;
  grid-gap: 5px;
  grid-template-areas:
    "status status "
    "cmd1 cmd2"
    "cmd3 cmd3";
}

.status {
  grid-area: status;
}

.cmd1 {
  grid-area: cmd1;
}

.cmd2 {
  grid-area: cmd2;
}

.cmd3 {
  grid-area: cmd3;
}
</style>