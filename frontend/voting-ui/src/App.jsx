import { useEffect, useState } from 'react';

function App() {
  const [votes, setVotes] = useState([]);
  const [option, setOption] = useState('Option A');

  const loadVotes = async () => {
    const res = await fetch('/api/vote');
    const data = await res.json();
    setVotes(data);
  };

  const submitVote = async () => {
    await fetch('/api/vote', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ option })
    });
    await loadVotes();
  };

  useEffect(() => { loadVotes(); }, []);

  return (
    <div>
      <h1>Голосование</h1>
      <select onChange={e => setOption(e.target.value)}>
        <option>Option A</option>
        <option>Option B</option>
        <option>Option C</option>
      </select>
      <button onClick={submitVote}>Проголосовать</button>
      <ul>
        {votes.map(v => (
          <li key={v.option}>{v.option}: {v.count}</li>
        ))}
      </ul>
    </div>
  );
}

export default App;